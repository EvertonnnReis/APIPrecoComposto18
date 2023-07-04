using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System;

namespace PainelGerencial.Domain.Fenix
{
    public class GroupingConverter : JsonConverter
    {
        public override bool CanRead => false;

        public override bool CanConvert(Type objectType) => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.Culture = CultureInfo.InvariantCulture;
            writer.DateFormatString = "s";

            JObject obj = new JObject();
            foreach (PropertyInfo pi in value.GetType().GetProperties().OrderBy(x => x.MetadataToken))
            {
                JToken token = JToken.FromObject(pi.GetValue(value) ?? JValue.CreateNull());
                GroupAttribute group = pi.GetCustomAttribute<GroupAttribute>();
                if (group != null)
                {
                    JObject groupObj = CreateJObject(obj, group.Name);
                    if (!string.IsNullOrEmpty(group.SubgroupName))
                    {
                        JObject subgroupObj = CreateJObject(groupObj, group.SubgroupName);
                        if (!string.IsNullOrEmpty(group.SubsubgroupName))
                        {
                            JObject subsubgroupObj = CreateJObject(subgroupObj, group.SubsubgroupName);
                            subsubgroupObj.Add(group.Value, token);
                            continue;
                        }
                        subgroupObj.Add(group.Value, token);
                        continue;
                    }
                    groupObj.Add(group.Value, token);
                    continue;
                }
                obj.Add(pi.Name, token);
            }
            obj.WriteTo(writer);

            // Para retornar o nome da classe como primeiro nível
            //new JObject(new JProperty(value.GetType().Name, obj)).WriteTo(writer);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        private JObject CreateJObject(JObject parentObj, string childName)
        {
            JObject childObj = (JObject)parentObj[childName];
            if (childObj == null)
            {
                childObj = new JObject();
                parentObj.Add(childName, childObj);
            }
            return childObj;
        }
    }
}

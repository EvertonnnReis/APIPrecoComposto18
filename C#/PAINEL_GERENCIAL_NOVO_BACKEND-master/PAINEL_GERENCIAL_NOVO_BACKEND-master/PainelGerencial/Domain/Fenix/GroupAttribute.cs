using System;

namespace PainelGerencial.Domain.Fenix
{
    public class GroupAttribute : Attribute
    {
        public string Name { get; set; }
        public string SubgroupName { get; set; }
        public string SubsubgroupName { get; set; }
        public string Value { get; set; }

        public GroupAttribute(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public GroupAttribute(string name, string subgroupName, string value)
        {
            Name = name;
            SubgroupName = subgroupName;
            Value = value;
        }

        public GroupAttribute(string name, string subgroupName, string subsubgroupName, string value)
        {
            Name = name;
            SubgroupName = subgroupName;
            SubsubgroupName = subsubgroupName;
            Value = value;
        }
    }
}

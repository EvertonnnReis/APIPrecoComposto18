using Newtonsoft.Json;

namespace ControleAcesso
{
    public class TokenSigeco_Model
    {
        [JsonProperty("access_token")]
        public string Token { get; set; }

        [JsonProperty("token_type")]
        public string TokenTipo { get; set; }

        [JsonProperty("expires_in")]
        public int ExpiraEm { get; set; }

        [JsonProperty("user_id")]
        public string UsuarioCodigo { get; set; }

        [JsonProperty("user_name")]
        public string UsuarioNome { get; set; }

        [JsonProperty("user_mail")]
        public string UsuarioEmail { get; set; }

        [JsonProperty(".issued")]
        public string DataGeracao { get; set; }

        [JsonProperty(".expires")]
        public string DataExpiracao { get; set; }
    }
}
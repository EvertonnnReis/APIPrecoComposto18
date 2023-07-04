using ControleAcesso;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RestSharp;

namespace PainelGerencial.Controller
{
    [ApiController]
    [Route("[controller]")]
    public class ControleAcessoController : ControllerBase
    {
        private const string _url = "http://api.dakotaparts.com.br:57137/";
        private static string _usuario;
        private static string _senha;

        public ControleAcessoController(IConfiguration configuration)
        {
            _usuario = configuration.GetValue("ControleAcesso:Usuario", "");
            _senha = configuration.GetValue("ControleAcesso:Senha", "");
        }

        [HttpGet]
        [Route("[action]")]
        public TokenSigeco_Model retornaToken()
        {
            const string recurso = "/Token";

            var client = new RestClient(_url);
            var request = new RestRequest(recurso, Method.Post);
            request.AddHeader("Accept", "application/json");
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("grant_type", "password");
            request.AddParameter("username", _usuario);
            request.AddParameter("password", _senha);
            RestResponse response = client.Execute(request);

            var token = JsonConvert.DeserializeObject<TokenSigeco_Model>(response.Content);
            
            return token;

        }
    }
}

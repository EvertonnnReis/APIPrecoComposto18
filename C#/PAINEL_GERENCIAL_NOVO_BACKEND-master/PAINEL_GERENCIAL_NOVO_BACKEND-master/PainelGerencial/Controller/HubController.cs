using AbacosWSERP;
using ControleAcesso;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PainelGerencial.Controller
{
    [ApiController]
    [Route("[controller]")]
    public class HubController : ControllerBase
    {
        private const string _url = "http://integra02.connectparts.com.br:8032/hub";
        //private const string _url = "http://localhost:1553";
        private readonly ControleAcessoController _controleAcesso;
        private static string _usuario;
        private static string _senha;

        public HubController(IConfiguration configuration, ControleAcessoController controleAcesso)
        {
            _usuario = configuration.GetValue("ControleAcesso:Usuario", "");
            _senha = configuration.GetValue("ControleAcesso:Senha", "");
            _controleAcesso = controleAcesso;
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<dynamic> AjusteVtex(string dk, string nome)
        {

            var token = _controleAcesso.retornaToken();

            string recurso = $"/Produto/GerarEstimuloProduto";

            var options = new RestClientOptions(_url)
            {
                MaxTimeout = -1,
            };
            var client = new RestClient(options);
            var request = new RestRequest("/Produto/GerarEstimuloProduto", Method.Post);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Accept", "application/json");
            request.AddHeader("Authorization", "bearer " + token.Token);

            request.AddParameter("Nome", nome, ParameterType.QueryString);

            var body = dk;
            request.AddBody(body);
            RestResponse response = await client.ExecuteAsync(request);
            
            var retorno = JsonConvert.DeserializeObject<dynamic>(response.Content);

            return retorno;
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Asn1.Crmf;
using PainelGerencial.Domain.ShoppingDePrecos;
using RestSharp;
using System;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PainelGerencial.Controller
{
    [ApiController]
    [Route("[controller]")]
    public class ShoppingDePrecosController : ControllerBase
    {
        const string _token = "Bearer YnJ1bm8ub3lhbWFkYUBjb25uZWN0cGFydHMuY29tLmJyOkNvbm5lY3RAMjAyMg==";
        private static HttpClient _client;
        private readonly IConfiguration _configuration;

        public ShoppingDePrecosController(IConfiguration configuration)
        {
            _configuration = configuration;

            _client = new HttpClient(new HttpClientHandler()
            {
                UseProxy = false,
                Proxy = null
            })
            {
                BaseAddress = new Uri(_configuration[$"ShoppingDePreco:Credential:BaseAddress"])
            };
        }

        [HttpGet]
        public async Task<IActionResult> plataformas()
        {
            var client = new RestClient("https://apiv2.shoppingdeprecos.com.br");
            var request = new RestRequest("/plataformas", Method.Get);
            RestResponse response = await client.ExecuteAsync(request);
            
            var resultado = JsonConvert.DeserializeObject<Plataforma>(response.Content);

            return Ok(resultado);
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> Etiqueta(string pedido, string integracao, string plataforma)
        {
            var client = new RestClient("https://api.shoppingdeprecos.com.br/");
            var request = new RestRequest("etiquetas", Method.Post);
            request.AlwaysMultipartFormData = true;
            request.AddParameter("email", "connect--356@SHOPPING");
            request.AddParameter("senha", "GYZuOtEV9MXQ");
            request.AddParameter("IntegracaoID", integracao);
            request.AddParameter("PlataformaID", plataforma);
            request.AddParameter("Pedidos", pedido);
            request.AddParameter("Formato", "ZPL");
            RestResponse response = await client.ExecuteAsync<dynamic>(request);

            return Ok(response.Content);
        }

        [NonAction]
        public async Task<HttpResponseMessage> ConfigureRequestAsync(string path
            , HttpMethod metodo
            , CancellationToken cancellationToken
            , StringContent body = null)
        {
            var request = new HttpRequestMessage(metodo, path)
            {
                Content = body
            };
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Add("Authorization", _token);

            var response = await _client.SendAsync(request, cancellationToken);
            var content = response.Content != null ? await response.Content.ReadAsStringAsync() : null;

            return response;
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> Pedido(string codigoExternoDoPedido, string integracao)
        {
            try
            {
                var path = $"/pedidos/{codigoExternoDoPedido}/integracao/{integracao}/dados";
                var response = await this.ConfigureRequestAsync(path, HttpMethod.Get, CancellationToken.None);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return Ok(response.Content);
                }
                return BadRequest();

            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}

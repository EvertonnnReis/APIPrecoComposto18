using Domain.Vtex;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PainelGerencial.Domain;
using PainelGerencial.Domain.Abacos;
using PainelGerencial.Domain.Vtex;
using PainelGerencial.Repository;
using PainelGerencial.Services;
using RecursosCompartilhados.ViewModels.Vtex;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace PainelGerencial.Controller
{
    [ApiController]
    [Route("[controller]")]
    public class VtexController : ControllerBase
    {
        private IVtexRepository _vtexRepository;
        private IAbacosRepository _abacosRepository;
        private IVtexServicos _vtexServicos;
        private const string _urlVtex = "https://connectparts.vtexcommercestable.com.br/";

        public VtexController(IVtexRepository repository, IAbacosRepository abacosRepository, IVtexServicos vtexServicos)
        {
            _vtexRepository = repository;
            _abacosRepository = abacosRepository;
            _vtexServicos = vtexServicos;
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<List<Feed_Model>> FeedV3()
        {
            var client = new RestClient("https://connectparts.vtexcommercestable.com.br/");
            var request = new RestRequest(resource: "api/orders/feed", method: Method.Get);
            request.AddHeader("Accept", "application/json");
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("X-VTEX-API-AppKey", "vtexappkey-connectparts-OBIOTU");
            request.AddHeader("X-VTEX-API-AppToken", "FUXRTIHFBZTQERILHBUEJEUOEXYJYBCTAPDFLHLCRCQDLQVXXYRSUJBYNEUDECPMLVMBPZQDCVSSLGMBCYZFHACKDUOJDNUBFEBCYZRTHBZDBOHXAWCZGOFTWSOGUJDS");
            request.AddParameter("maxlot", "10");
            RestResponse response = await client.ExecuteAsync(request);

            List<Feed_Model> feed = JsonConvert.DeserializeObject<List<Feed_Model>>(response.Content);

            return feed;
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> CommitFeedV3(IList<string> handles)
        {
            var client = new RestClient("https://connectparts.vtexcommercestable.com.br/");
            var request = new RestRequest(resource: "api/orders/feed", method: Method.Post);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Accept", "application/json");
            request.AddHeader("X-VTEX-API-AppKey", "vtexappkey-connectparts-OBIOTU");
            request.AddHeader("X-VTEX-API-AppToken", "FUXRTIHFBZTQERILHBUEJEUOEXYJYBCTAPDFLHLCRCQDLQVXXYRSUJBYNEUDECPMLVMBPZQDCVSSLGMBCYZFHACKDUOJDNUBFEBCYZRTHBZDBOHXAWCZGOFTWSOGUJDS");
            request.AddBody(new { handles = handles });
            RestResponse response = await client.ExecuteAsync(request);

            return Ok(response.Content);
        }

        [HttpPost]
        [Route("[action]")]
        public IActionResult InserePedidoFila(PedidoFila_Model pedidoFila)
        {
            List<PedidoFila_Model> pedido = (List<PedidoFila_Model>)
                _vtexRepository.GetPedidoFila(pedidoFila.CodigoExterno);

            int captado = 0;
            if (pedido.Count == 0)
            {
                captado = _vtexRepository.InserePedidoFila(pedidoFila);
            }

            if (captado == 0)
            {
                return BadRequest(new 
                {
                    status = "Ocorreu um erro ao inserir o pedido"
                });
            }

            return Ok(new { status = "Pedido integrado." });
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<PedidoVtexViewModel> PedidoVtexUnico(string codigoVtex)
        {
            PedidoVtexViewModel pedidoVtex = await _vtexServicos.PedidoVtexUnico(codigoVtex);

            return pedidoVtex;
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> ConsomePedidos()
        {
            List<Feed_Model> feedV3 = await FeedV3();
            List<string> handles = new List<string>();
            
            foreach(Feed_Model item in feedV3)
            {
                InserePedidoFila(await _vtexServicos.ConvertePedido(item));
                handles.Add(item.handle);
            }

            // comita os pedidos
            await CommitFeedV3(handles);

            return Ok(new
            {
                status = "Pedidos integrados.",
                quantidade = feedV3.Count,
                pedidos = feedV3,
            });
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> PedidosProntosParaManuseio()
        {
            List<PedidoFila_Model> fila = _vtexRepository.PedidosProntosParaManuseio();

            return Ok(fila);
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> IntegrarPedidos([FromHeader] string token)
        {
            List<PedidoFila_Model> fila = _vtexRepository.PedidosProntosParaManuseio();

            List<PedidoFila_Model> integrados = new List<PedidoFila_Model>();
            List<PedidoFila_Model> naoIntegrados = new List<PedidoFila_Model>();

            foreach (var item in fila)
            {
                List<TCOM_PEDSAI> pedidoIntegrado = (List<TCOM_PEDSAI>)_abacosRepository.PedidoIntegrado(item.CodigoExterno);
                if (pedidoIntegrado.Any())
                {
                    int integrado = _vtexRepository.AtualizaPedidoIntegrado(item.CodigoExterno);
                    integrados.Add(item);
                    continue;
                }

                var retorno = await _vtexServicos.IntegrarPedido(token, item.CodigoExterno);


                pedidoIntegrado = (List<TCOM_PEDSAI>)_abacosRepository.PedidoIntegrado(item.CodigoExterno);
                var statusVtex = await this.PedidoVtexUnico(item.CodigoVtex);

                if (pedidoIntegrado.Any() && statusVtex.Status != "ready-for-handling")
                {
                    int integrado = _vtexRepository.AtualizaPedidoIntegrado(item.CodigoExterno);
                    if (integrado > 0) integrados.Add(item);
                } 
                else
                {
                    naoIntegrados.Add(item);
                }
            }

            return Ok(new
            {
                quantidade_integrados = integrados.Count,
                quantidade_naoIntegrados = naoIntegrados.Count,
                integrados = integrados,
                naoIntegrados = naoIntegrados,
            });
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> BuscarSkusPorRefIds(IList<int> refIds)
        {
            var client = new RestClient(_urlVtex);
            var request = new RestRequest("api/catalog_system/pub/sku/stockkeepingunitidsbyrefids", Method.Post);
            request.AddHeader("Accept", "application/json");
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("X-VTEX-API-AppKey", "vtexappkey-connectparts-OBIOTU");
            request.AddHeader("X-VTEX-API-AppToken", "FUXRTIHFBZTQERILHBUEJEUOEXYJYBCTAPDFLHLCRCQDLQVXXYRSUJBYNEUDECPMLVMBPZQDCVSSLGMBCYZFHACKDUOJDNUBFEBCYZRTHBZDBOHXAWCZGOFTWSOGUJDS");
            var body = JsonConvert.SerializeObject(refIds);
            request.AddParameter("application/json", body, ParameterType.RequestBody);
            RestResponse response = client.Execute(request);

            return Ok(response.Content);
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<ProdutoVtexViewModel> ObterProdutoPorCodigoReferencia(string refId)
        {
            ProdutoVtexViewModel refIds = await _vtexServicos.ObterProdutoPorCodigoReferencia(refId);
            
            return refIds;
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<ProdutoVtexViewModel> CriarProduto(ProdutoVtexViewModel produto)
        {
            ProdutoVtexViewModel resposta = await _vtexServicos.CriarProduto(produto);
            
            return resposta;
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<ProdutoVtexViewModel> AtualizarProduto(ProdutoVtexViewModel produto)
        {
            ProdutoVtexViewModel resposta = await _vtexServicos.AtualizarProduto(produto);
            
            return resposta;
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<SkuByRefIdVtexViewModel> CriarSku(SkuByRefIdVtexViewModel produto)
        {
            SkuByRefIdVtexViewModel resposta = await _vtexServicos.CriarSku(produto);
            
            return resposta;
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<SkuByRefIdVtexViewModel> AtualizarSku(SkuByRefIdVtexViewModel produto)
        {
            SkuByRefIdVtexViewModel resposta = await _vtexServicos.AtualizarSku(produto);
            
            return resposta;
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<string> AtualizarEan(int skuId, string ean)
        {
            string resposta = await _vtexServicos.AtualizarEan(skuId, ean);
            
            return resposta;
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<SkuByRefIdVtexViewModel> SkuPorRefId(string codigoExterno)
        {
            SkuByRefIdVtexViewModel resposta = await _vtexServicos.SkuPorRefId(codigoExterno);
            
            return resposta;
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<List<StockKeepingUnitKit>> SkuListaKitPorParentId(int skuPai)
        {
            List<StockKeepingUnitKit> resposta = await _vtexServicos.SkuListaKitPorParentId(skuPai);
            
            return resposta;
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<StockKeepingUnitKit> CriarSkuKit(StockKeepingUnitKit kit)
        {
            var resposta = await _vtexServicos.CriarSkuKit(kit);
            
            return resposta;
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<StockKeepingUnitKit> ExisteProduto(StockKeepingUnitKit kit)
        {
            var resposta = await _vtexServicos.ExisteProduto(kit);
            
            return resposta;
        }

        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> ImagensProdutos(string dk)
        {
            var codigoExterno = await _vtexServicos.SkuPorRefId(dk);

            if (codigoExterno.Id == null)
            {
                return NotFound();
            }
            var imagensProdutos = await _vtexServicos.ImagensProdutosPorId((int)codigoExterno.Id);
            
            return Ok(imagensProdutos);
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> IntegrarImagemUnica(string dk)
        {
            var retorno = await _vtexServicos.IntegrarAnuncioUnico(dk);
            if (retorno != null)
            {
                return Ok(retorno);
            }
            return BadRequest(new
            {
                erro = "Código não encontrado."
            });
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> IntegrarImagens()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            try
            {
                var retorno = await _vtexServicos.IntegrarAnuncios();
                if (retorno != null)
                {
                    sw.Stop();
                    return Ok(new { integrados = retorno.Count(), tempo = sw.Elapsed });
                }
            } 
            catch(Exception ex)
            {
                sw.Stop();
                return BadRequest(ex);
            }
            sw.Stop();
            return BadRequest();
        }
    }
}

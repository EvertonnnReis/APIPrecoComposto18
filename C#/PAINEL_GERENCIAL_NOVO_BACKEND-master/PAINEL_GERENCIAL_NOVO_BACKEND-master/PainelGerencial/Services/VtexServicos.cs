using Dapper;
using Domain.Vtex;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;
using PainelGerencial.Domain;
using PainelGerencial.Domain.Vtex;
using PainelGerencial.Repository;
using RecursosCompartilhados.ViewModels.Vtex;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace PainelGerencial.Services
{
    public class VtexServicos : IVtexServicos
    {
        private const string _urlVtex = "https://connectparts.vtexcommercestable.com.br/";
        private IVtexRepository _vtexRepository;

        public VtexServicos(IVtexRepository vtexRepository)
        {
            _vtexRepository = vtexRepository;
        }

        public async Task<PedidoVtexViewModel> PedidoVtexUnico(string codigoVtex)
        {
            var client = new RestClient(_urlVtex);
            var request = new RestRequest("api/oms/pvt/orders/" + codigoVtex, Method.Get);
            request.AddHeader("Accept", "application/json");
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("X-VTEX-API-AppKey", "vtexappkey-connectparts-OBIOTU");
            request.AddHeader("X-VTEX-API-AppToken", "FUXRTIHFBZTQERILHBUEJEUOEXYJYBCTAPDFLHLCRCQDLQVXXYRSUJBYNEUDECPMLVMBPZQDCVSSLGMBCYZFHACKDUOJDNUBFEBCYZRTHBZDBOHXAWCZGOFTWSOGUJDS");
            RestResponse response = await client.ExecuteAsync(request);

            PedidoVtexViewModel pedidoVtex = JsonConvert.DeserializeObject<PedidoVtexViewModel>(response.Content);

            return pedidoVtex;
        }

        public async Task<PedidoFila_Model> ConvertePedido(Feed_Model feed)
        {
            if (feed == null)
            {
                return null;
            }
            PedidoFila_Model pedidoFila = new PedidoFila_Model();
            pedidoFila.CodigoVtex = feed.orderId;
            PedidoVtexViewModel pedidoVtex = await PedidoVtexUnico(feed.orderId);
            pedidoFila.CodigoExterno = pedidoVtex.Sequencia;
            pedidoFila.Estado = feed.state;

            return pedidoFila;
        }

        public async Task<bool> IntegrarPedido(string token, string codigoExterno)
        {
            var client = new RestClient("http://api.dakotaparts.com.br:49998/");
            var request = new RestRequest("Pedido/IntegrarPedido?pedido=" + codigoExterno, Method.Post);
            request.AddHeader("Accept", "application/json");
            request.AddHeader("Authorization", token);
            RestResponse response = await client.ExecuteAsync(request);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return false;
            }
            return true;
        }

        public async Task<IEnumerable<Produto_Model>> ImagensProdutos(string dk)
        {
            var client = new RestClient(_urlVtex);
            var request = new RestRequest("api/catalog_system/pub/products/search", Method.Get);
            request.AddHeader("Accept", "application/json");
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("X-VTEX-API-AppKey", "vtexappkey-connectparts-GOFEYM");
            request.AddHeader("X-VTEX-API-AppToken", "CPMDLDCNXHAIISITKRMVGASHKLZUKNEVCVAEQGOQOEKGDUDKWEBSAVQANTBWVUMEUIHGGOCIRMVMDYKZGTMNIMROFRFTMOIDAYRTPZLMICIBPGYWUWHMLJCVXCKVECRB");

            request.AddParameter("fq", "alternateIds_RefId:" + dk);

            RestResponse response = await client.ExecuteAsync(request);

            var retorno = response.Content != null 
                ? JsonConvert.DeserializeObject<IEnumerable<Produto_Model>>(response.Content) 
                : null;

            return retorno;
        }

        public async Task<StockKeepingUnit_Model> ImagensProdutosPorId(int skuId)
        {
            var client = new RestClient(_urlVtex);
            var request = new RestRequest($"/api/catalog_system/pvt/sku/stockkeepingunitbyid/{skuId}", Method.Get);
            request.AddHeader("Accept", "application/json");
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("X-VTEX-API-AppKey", "vtexappkey-connectparts-GOFEYM");
            request.AddHeader("X-VTEX-API-AppToken", "CPMDLDCNXHAIISITKRMVGASHKLZUKNEVCVAEQGOQOEKGDUDKWEBSAVQANTBWVUMEUIHGGOCIRMVMDYKZGTMNIMROFRFTMOIDAYRTPZLMICIBPGYWUWHMLJCVXCKVECRB");

            RestResponse response = await client.ExecuteAsync(request);

            try
            {
                var retorno = response.Content != null
                    ? JsonConvert.DeserializeObject<StockKeepingUnit_Model>(response.Content)
                    : null;

                return retorno;
            } 
            catch(Exception ex)
            {
                return null;
            }
        }

        public async Task<List<Vtex_Anuncios_Fotos_Model>> AnuncioFoto(Produto_Model produto)
        {
            List<Vtex_Anuncios_Fotos_Model> listaAnuncios = new List<Vtex_Anuncios_Fotos_Model>();

            produto.items.ForEach(item =>
            {
                Vtex_Anuncios_Fotos_Model anuncio = new Vtex_Anuncios_Fotos_Model();
                anuncio.id_vtex = produto.productId;

                if (item.referenceId != null)
                {
                    anuncio.sku = item.referenceId[0]?.Value;
                }                

                if (item.images != null)
                {
                    for (int i = 0; i < item.images.Count; i++)
                    {
                        switch (i)
                        {
                            case 0:
                                anuncio.foto1 = item.images[i].imageUrl;
                                break;
                            case 1:
                                anuncio.foto2 = item.images[i].imageUrl;
                                break;
                            case 2:
                                anuncio.foto3 = item.images[i].imageUrl;
                                break;
                            case 3:
                                anuncio.foto4 = item.images[i].imageUrl;
                                break;
                            case 4:
                                anuncio.foto5 = item.images[i].imageUrl;
                                break;
                            case 5:
                                anuncio.foto6 = item.images[i].imageUrl;
                                break;
                            case 6:
                                anuncio.foto7 = item.images[i].imageUrl;
                                break;
                            case 7:
                                anuncio.foto8 = item.images[i].imageUrl;
                                break;
                            case 8:
                                anuncio.foto9 = item.images[i].imageUrl;
                                break;
                            case 9:
                                anuncio.foto10 = item.images[i].imageUrl;
                                break;
                            case 10:
                                anuncio.foto11 = item.images[i].imageUrl;
                                break;
                            case 11:
                                anuncio.foto12 = item.images[i].imageUrl;
                                break;
                            case 12:
                                anuncio.foto13 = item.images[i].imageUrl;
                                break;
                            case 13:
                                anuncio.foto14 = item.images[i].imageUrl;
                                break;
                            default:
                                break;
                        }
                    }

                    listaAnuncios.Add(anuncio);
                }                
            });

            return listaAnuncios;
        }

        public async Task<Vtex_Anuncios_Fotos_Model> AnuncioFoto(StockKeepingUnit_Model produto, string dk)
        {
            Vtex_Anuncios_Fotos_Model anuncio = new Vtex_Anuncios_Fotos_Model();

            anuncio.id_vtex = produto.Id;
            anuncio.sku = dk;

            if (produto.Images != null)
            {
                for (int i = 0; i < produto.Images.Count; i++)
                {
                    switch (i)
                    {
                        case 0:
                            anuncio.foto1 = produto.Images[i].imageUrl;
                            break;
                        case 1:
                            anuncio.foto2 = produto.Images[i].imageUrl;
                            break;
                        case 2:
                            anuncio.foto3 = produto.Images[i].imageUrl;
                            break;
                        case 3:
                            anuncio.foto4 = produto.Images[i].imageUrl;
                            break;
                        case 4:
                            anuncio.foto5 = produto.Images[i].imageUrl;
                            break;
                        case 5:
                            anuncio.foto6 = produto.Images[i].imageUrl;
                            break;
                        case 6:
                            anuncio.foto7 = produto.Images[i].imageUrl;
                            break;
                        case 7:
                            anuncio.foto8 = produto.Images[i].imageUrl;
                            break;
                        case 8:
                            anuncio.foto9 = produto.Images[i].imageUrl;
                            break;
                        case 9:
                            anuncio.foto10 = produto.Images[i].imageUrl;
                            break;
                        case 10:
                            anuncio.foto11 = produto.Images[i].imageUrl;
                            break;
                        case 11:
                            anuncio.foto12 = produto.Images[i].imageUrl;
                            break;
                        case 12:
                            anuncio.foto13 = produto.Images[i].imageUrl;
                            break;
                        case 13:
                            anuncio.foto14 = produto.Images[i].imageUrl;
                            break;
                        default:
                            break;
                    }
                }
            }

            return anuncio;
        }

        public async Task InserirProdutoImagemUnico(Vtex_Anuncios_Fotos_Model anuncio)
        {
            _vtexRepository.InserirProdutoImagem(anuncio);
        }

        public async Task InserirProdutoImagemLote(List<Vtex_Anuncios_Fotos_Model> listaAnuncios)
        {
            foreach(var anuncio in listaAnuncios)
            {
                _vtexRepository.InserirProdutoImagem(anuncio);
            }
        }

        public async Task<Vtex_Anuncios_Fotos_Model> IntegrarAnuncioUnico(string dk)
        {
            var sku = await this.SkuPorRefId(dk);
            if (sku == null)
            {
                return null;
            }
            var imagensProdutos = await this.ImagensProdutosPorId((int)sku.Id);
            
            if (imagensProdutos != null)
            {
                var anuncioVtex = await this.AnuncioFoto(imagensProdutos, dk);

                this.InserirProdutoImagemUnico(anuncioVtex);

                return anuncioVtex;
            }
            return null;
        }

        public async Task<List<Vtex_Anuncios_Fotos_Model>> IntegrarAnuncios()
        {
            List<string> dks = _vtexRepository.RetornaDks();
            List<Vtex_Anuncios_Fotos_Model> listaAnunciosVtex = new List<Vtex_Anuncios_Fotos_Model>();

            foreach(var dk in dks)
            {
                int conversao;
                bool sucesso = int.TryParse(dk, out conversao);
                if (sucesso)
                {
                    var sku = await this.SkuPorRefId(dk);
                    if (sku == null)
                    {
                        continue;
                    }
                    
                    var imagensProdutos = await this.ImagensProdutosPorId((int)sku.Id);
                    var imagem = imagensProdutos;

                    if (imagem != null)
                    {
                        var anuncioVtex = await this.AnuncioFoto(imagem, dk);

                        this.InserirProdutoImagemUnico(anuncioVtex);

                        listaAnunciosVtex.Add(anuncioVtex);
                    }
                }
            }

            return listaAnunciosVtex;
        }

        public async Task<ProdutoVtexViewModel> ObterProdutoPorCodigoReferencia(string refId)
        {
            var client = new RestClient(_urlVtex);
            string recurso = $"api/catalog_system/pvt/products/productgetbyrefid/{refId}";
            var request = new RestRequest(recurso, Method.Get);
            request.AddHeader("Accept", "application/json");
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("X-VTEX-API-AppKey", "vtexappkey-connectparts-GOFEYM");
            request.AddHeader("X-VTEX-API-AppToken", "CPMDLDCNXHAIISITKRMVGASHKLZUKNEVCVAEQGOQOEKGDUDKWEBSAVQANTBWVUMEUIHGGOCIRMVMDYKZGTMNIMROFRFTMOIDAYRTPZLMICIBPGYWUWHMLJCVXCKVECRB");
            RestResponse response = await client.ExecuteAsync(request);
            Console.WriteLine(response.Content);

            ProdutoVtexViewModel refIds = JsonConvert.DeserializeObject<ProdutoVtexViewModel>(response.Content);

            return refIds;
        }

        public async Task<SkuByRefIdVtexViewModel> SkuPorRefId(string codigoExterno)
        {
            var recurso = $"api/catalog/pvt/stockkeepingunit";

            var client = new RestClient(_urlVtex);
            var request = new RestRequest(recurso, Method.Get);
            request.AddHeader("X-VTEX-API-AppKey", "vtexappkey-connectparts-GOFEYM");
            request.AddHeader("X-VTEX-API-AppToken", "CPMDLDCNXHAIISITKRMVGASHKLZUKNEVCVAEQGOQOEKGDUDKWEBSAVQANTBWVUMEUIHGGOCIRMVMDYKZGTMNIMROFRFTMOIDAYRTPZLMICIBPGYWUWHMLJCVXCKVECRB");
            request.AddHeader("accept", "application/json");
            request.AddHeader("content-type", "application/json");
            request.AddParameter("refId", codigoExterno);


            RestResponse response = client.Execute(request);

            SkuByRefIdVtexViewModel resposta = JsonConvert.DeserializeObject<SkuByRefIdVtexViewModel>(response.Content);

            return resposta;
        }

        public async Task<ProdutoVtexViewModel> CriarProduto(ProdutoVtexViewModel produto)
        {
            const string recurso = "api/catalog/pvt/product";
            var client = new RestClient(_urlVtex);
            var request = new RestRequest(recurso, Method.Post);
            request.AddHeader("accept", "application/json");
            request.AddHeader("content-type", "application/json");
            request.AddHeader("X-VTEX-API-AppKey", "vtexappkey-connectparts-GOFEYM");
            request.AddHeader("X-VTEX-API-AppToken", "CPMDLDCNXHAIISITKRMVGASHKLZUKNEVCVAEQGOQOEKGDUDKWEBSAVQANTBWVUMEUIHGGOCIRMVMDYKZGTMNIMROFRFTMOIDAYRTPZLMICIBPGYWUWHMLJCVXCKVECRB");

            var body = JsonConvert.SerializeObject(produto);
            request.AddParameter("application/json", body, ParameterType.RequestBody);

            RestResponse response = await client.ExecuteAsync(request);

            ProdutoVtexViewModel resposta = JsonConvert.DeserializeObject<ProdutoVtexViewModel>(response.Content);

            return resposta;
        }

        public async Task<ProdutoVtexViewModel> AtualizarProduto(ProdutoVtexViewModel produto)
        {
            var recurso = $"api/catalog/pvt/product/{produto.Id}";

            var client = new RestClient(_urlVtex);
            var request = new RestRequest(recurso, Method.Put);
            request.AddHeader("X-VTEX-API-AppKey", "vtexappkey-connectparts-GOFEYM");
            request.AddHeader("X-VTEX-API-AppToken", "CPMDLDCNXHAIISITKRMVGASHKLZUKNEVCVAEQGOQOEKGDUDKWEBSAVQANTBWVUMEUIHGGOCIRMVMDYKZGTMNIMROFRFTMOIDAYRTPZLMICIBPGYWUWHMLJCVXCKVECRB");
            request.AddHeader("accept", "application/json");
            request.AddHeader("content-type", "application/json");

            var corpo = JsonConvert.SerializeObject(produto);
            request.AddParameter("application/json", corpo, ParameterType.RequestBody);
            RestResponse response = client.Execute(request);

            ProdutoVtexViewModel resposta = JsonConvert.DeserializeObject<ProdutoVtexViewModel>(response.Content);

            return resposta;
        }

        public async Task<SkuByRefIdVtexViewModel> CriarSku(SkuByRefIdVtexViewModel produto)
        {
            var recurso = $"api/catalog/pvt/stockkeepingunit";

            var client = new RestClient(_urlVtex);
            var request = new RestRequest(recurso, Method.Post);
            request.AddHeader("X-VTEX-API-AppKey", "vtexappkey-connectparts-GOFEYM");
            request.AddHeader("X-VTEX-API-AppToken", "CPMDLDCNXHAIISITKRMVGASHKLZUKNEVCVAEQGOQOEKGDUDKWEBSAVQANTBWVUMEUIHGGOCIRMVMDYKZGTMNIMROFRFTMOIDAYRTPZLMICIBPGYWUWHMLJCVXCKVECRB");
            request.AddHeader("accept", "application/json");
            request.AddHeader("content-type", "application/json");

            var corpo = JsonConvert.SerializeObject(produto);
            request.AddParameter("application/json", corpo, ParameterType.RequestBody);
            RestResponse response = client.Execute(request);

            SkuByRefIdVtexViewModel resposta = JsonConvert.DeserializeObject<SkuByRefIdVtexViewModel>(response.Content);

            return resposta;
        }

        public async Task<string> AtualizarEan(int skuId, string ean)
        {
            var recurso = $"api/catalog/pvt/stockkeepingunit/{skuId}/ean/{ean}";

            var client = new RestClient(_urlVtex);
            var request = new RestRequest(recurso, Method.Post);
            request.AddHeader("X-VTEX-API-AppKey", "vtexappkey-connectparts-GOFEYM");
            request.AddHeader("X-VTEX-API-AppToken", "CPMDLDCNXHAIISITKRMVGASHKLZUKNEVCVAEQGOQOEKGDUDKWEBSAVQANTBWVUMEUIHGGOCIRMVMDYKZGTMNIMROFRFTMOIDAYRTPZLMICIBPGYWUWHMLJCVXCKVECRB");
            request.AddHeader("accept", "application/json");
            request.AddHeader("content-type", "application/json");

            request.AddParameter("skuId", skuId);
            request.AddParameter("ean", ean);
            RestResponse response = client.Execute(request);

            return response.Content.ToString();
        }

        public async Task<List<StockKeepingUnitKit>> SkuListaKitPorParentId(int skuPai)
        {
            var recurso = $"api/catalog/pvt/stockkeepingunitkit";

            var client = new RestClient(_urlVtex);
            var request = new RestRequest(recurso, Method.Get);
            request.AddHeader("X-VTEX-API-AppKey", "vtexappkey-connectparts-GOFEYM");
            request.AddHeader("X-VTEX-API-AppToken", "CPMDLDCNXHAIISITKRMVGASHKLZUKNEVCVAEQGOQOEKGDUDKWEBSAVQANTBWVUMEUIHGGOCIRMVMDYKZGTMNIMROFRFTMOIDAYRTPZLMICIBPGYWUWHMLJCVXCKVECRB");
            request.AddHeader("accept", "application/json");
            request.AddHeader("content-type", "application/json");
            request.AddParameter("parentSkuId", skuPai);


            RestResponse response = client.Execute(request);

            List<StockKeepingUnitKit> resposta = JsonConvert.DeserializeObject<List<StockKeepingUnitKit>>(response.Content);

            return resposta;
        }

        public async Task<StockKeepingUnitKit> ExisteProduto(StockKeepingUnitKit kit)
        {
            string recurso = $"api/catalog/pvt/stockkeepingunitkit/{kit.Id}";

            var client = new RestClient(_urlVtex);
            var request = new RestRequest(recurso, Method.Get);
            request.AddHeader("Accept", "application/json");
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("X-VTEX-API-AppKey", "vtexappkey-connectparts-GOFEYM");
            request.AddHeader("X-VTEX-API-AppToken", "CPMDLDCNXHAIISITKRMVGASHKLZUKNEVCVAEQGOQOEKGDUDKWEBSAVQANTBWVUMEUIHGGOCIRMVMDYKZGTMNIMROFRFTMOIDAYRTPZLMICIBPGYWUWHMLJCVXCKVECRB");

            var body = JsonConvert.SerializeObject(kit);
            request.AddParameter("application/json", body, ParameterType.RequestBody);

            RestResponse response = client.Execute(request);

            var resposta = JsonConvert
                .DeserializeObject<StockKeepingUnitKit>(response.Content);

            return resposta;
        }

        public async Task<StockKeepingUnitKit> CriarSkuKit(StockKeepingUnitKit kit)
        {
            const string recurso = "api/catalog/pvt/stockkeepingunitkit";

            var client = new RestClient(_urlVtex);
            var request = new RestRequest(recurso, Method.Post);
            request.AddHeader("Accept", "application/json");
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("X-VTEX-API-AppKey", "vtexappkey-connectparts-GOFEYM");
            request.AddHeader("X-VTEX-API-AppToken", "CPMDLDCNXHAIISITKRMVGASHKLZUKNEVCVAEQGOQOEKGDUDKWEBSAVQANTBWVUMEUIHGGOCIRMVMDYKZGTMNIMROFRFTMOIDAYRTPZLMICIBPGYWUWHMLJCVXCKVECRB");

            var body = JsonConvert.SerializeObject(kit);
            request.AddParameter("application/json", body, ParameterType.RequestBody);

            RestResponse response = client.Execute(request);

            var resposta = JsonConvert
                .DeserializeObject<StockKeepingUnitKit>(response.Content);

            return resposta;
        }

        public async Task<SkuByRefIdVtexViewModel> AtualizarSku(SkuByRefIdVtexViewModel produto)
        {
            var recurso = $"api/catalog/pvt/stockkeepingunit/{produto.Id}";

            var client = new RestClient(_urlVtex);
            var request = new RestRequest(recurso, Method.Put);
            request.AddHeader("X-VTEX-API-AppKey", "vtexappkey-connectparts-GOFEYM");
            request.AddHeader("X-VTEX-API-AppToken", "CPMDLDCNXHAIISITKRMVGASHKLZUKNEVCVAEQGOQOEKGDUDKWEBSAVQANTBWVUMEUIHGGOCIRMVMDYKZGTMNIMROFRFTMOIDAYRTPZLMICIBPGYWUWHMLJCVXCKVECRB");
            request.AddHeader("accept", "application/json");
            request.AddHeader("content-type", "application/json");

            var corpo = JsonConvert.SerializeObject(produto);
            request.AddParameter("application/json", corpo, ParameterType.RequestBody);
            RestResponse response = client.Execute(request);

            SkuByRefIdVtexViewModel resposta = JsonConvert.DeserializeObject<SkuByRefIdVtexViewModel>(response.Content);

            return resposta;
        }


        //Requests para acompanhamento de sellers...................................................................

        public async Task<ListSellersModel> Get_Lista_Sellers_Ativos(List<SellerParamModel> parametros)
        {
            var client = new RestClient(_urlVtex);
            var request = new RestRequest("api/seller-register/pvt/sellers", Method.Get);
            request.AddParameter("isActive", "true");
            if (parametros.Any() && parametros != null)
            {
                foreach (var item in parametros)
                {
                    request.AddParameter(item.Key, item.Value);
                }
            }
            request.AddHeader("Accept", "application/json");
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("X-VTEX-API-AppKey", "vtexappkey-connectparts-GOFEYM");
            request.AddHeader("X-VTEX-API-AppToken", "CPMDLDCNXHAIISITKRMVGASHKLZUKNEVCVAEQGOQOEKGDUDKWEBSAVQANTBWVUMEUIHGGOCIRMVMDYKZGTMNIMROFRFTMOIDAYRTPZLMICIBPGYWUWHMLJCVXCKVECRB");
            RestResponse response = await client.ExecuteAsync(request);

            ListSellersModel listaSellers = JsonConvert.DeserializeObject<ListSellersModel>(response.Content);
            return listaSellers;
        }

        public async Task<dynamic> Get_PedidosSeller()
        {
            TbPedidosModel ultimoPedido = new TbPedidosModel();
            ultimoPedido = _vtexRepository.buscaUltimoPedido();
            string dataInicial = "";
            if (ultimoPedido == null)
            {
                dataInicial = DateTime.Now.ToString("yyyy-MM-ddT00:00:00.000Z");
            }
            else
            {
                dataInicial = ultimoPedido.CreationDate.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            }
            var client = new RestClient(_urlVtex);
            var request = new RestRequest("api/oms/pvt/orders", Method.Get);
            DateTime dataHoraAtual = DateTime.Now;
            request.AddParameter("f_creationDate", $"creationDate:[{dataInicial} TO {dataHoraAtual.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")}]");
            request.AddHeader("Accept", "application/json");
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("X-VTEX-API-AppKey", "vtexappkey-connectparts-GOFEYM");
            request.AddHeader("X-VTEX-API-AppToken", "CPMDLDCNXHAIISITKRMVGASHKLZUKNEVCVAEQGOQOEKGDUDKWEBSAVQANTBWVUMEUIHGGOCIRMVMDYKZGTMNIMROFRFTMOIDAYRTPZLMICIBPGYWUWHMLJCVXCKVECRB");
            RestResponse response = await client.ExecuteAsync(request);

            PedidosSellerListModel Pedidos = JsonConvert.DeserializeObject<PedidosSellerListModel>(response.Content);
            return Pedidos;
        }

        public async Task<dynamic> GravaPedidos(PedidosSellerListModel ListPedidos)
        {
            List<PedidosSellerModel> pedidoSeller = new List<PedidosSellerModel>();
            foreach(var item in ListPedidos.List)
            {
                var client = new RestClient(_urlVtex);
                var request = new RestRequest($"api/oms/pvt/orders/{item.OrderId}", Method.Get);
                request.AddHeader("Accept", "application/json");
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("X-VTEX-API-AppKey", "vtexappkey-connectparts-GOFEYM");
                request.AddHeader("X-VTEX-API-AppToken", "CPMDLDCNXHAIISITKRMVGASHKLZUKNEVCVAEQGOQOEKGDUDKWEBSAVQANTBWVUMEUIHGGOCIRMVMDYKZGTMNIMROFRFTMOIDAYRTPZLMICIBPGYWUWHMLJCVXCKVECRB");
                RestResponse response = await client.ExecuteAsync(request);
                PedidosSellerModel Pedidos = JsonConvert.DeserializeObject<PedidosSellerModel>(response.Content);
                Pedidos.CreationDate = new DateTime(Pedidos.CreationDate.Ticks / 10000 * 10000);
                Pedidos.LastChange = new DateTime(Pedidos.LastChange.Ticks / 10000 * 10000);
                pedidoSeller.Add(Pedidos);
            }

            
            List<TbPedidosModel> NovosPedidos = new List<TbPedidosModel>();
            dynamic Seller = null;
            foreach (var _Pedidos in pedidoSeller)
            {
                Seller = _Pedidos.Sellers.FirstOrDefault();
                if ( Seller != null && Seller.Id != "1")
                {
                    NovosPedidos.Add(new()
                    {
                        OrderID = _Pedidos.OrderId,
                        CreationDate = _Pedidos.CreationDate,
                        Status = _Pedidos.Status,
                        StatusDescription = _Pedidos.StatusDescription,
                        LastChange = _Pedidos.LastChange,
                        SellerID = Seller.Id
                    });
                }
            }

            var result = await _vtexRepository.NovosPedidos(NovosPedidos);

            return new { result };
        }

        public async Task<dynamic> checaStatusPedidos()
        {
            List<TbPedidosModel> pedidos = _vtexRepository.buscaTodos();
            dynamic result = null;
            foreach (var item in pedidos)
            {
                var client = new RestClient(_urlVtex);
                var request = new RestRequest($"api/oms/pvt/orders/{item.OrderID}", Method.Get);
                request.AddHeader("Accept", "application/json");
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("X-VTEX-API-AppKey", "vtexappkey-connectparts-OBIOTU");
                request.AddHeader("X-VTEX-API-AppToken", "FUXRTIHFBZTQERILHBUEJEUOEXYJYBCTAPDFLHLCRCQDLQVXXYRSUJBYNEUDECPMLVMBPZQDCVSSLGMBCYZFHACKDUOJDNUBFEBCYZRTHBZDBOHXAWCZGOFTWSOGUJDS");
                RestResponse response = await client.ExecuteAsync(request);
                PedidosSellerModel Pedidos = JsonConvert.DeserializeObject<PedidosSellerModel>(response.Content);

                //string trataData = Pedidos.LastChange.ToString();
                Pedidos.LastChange = new DateTime(Pedidos.LastChange.Ticks / 10000 * 10000);

                List<TbPedidosModel> UpdatePedidos = new List<TbPedidosModel>();
                if (Pedidos.LastChange != item.LastChange)
                {
                    UpdatePedidos.Add(new()
                    {
                        OrderID = Pedidos.OrderId,
                        CreationDate = Pedidos.CreationDate,
                        Status = Pedidos.Status,
                        StatusDescription = Pedidos.StatusDescription,
                        LastChange = Pedidos.LastChange,
                        SellerID = Pedidos.Sellers.FirstOrDefault().Id
                    });
                    result = new { Pedidos = await _vtexRepository.UpdatePedidos(UpdatePedidos), lastChangeBanco = item.LastChange, lastChangeAPI = Pedidos.LastChange };
                }
            }
            return result;
        }
    }
}

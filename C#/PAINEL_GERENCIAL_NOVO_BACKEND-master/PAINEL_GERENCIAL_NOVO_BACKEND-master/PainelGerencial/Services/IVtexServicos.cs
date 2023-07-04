using Domain.Vtex;
using Microsoft.AspNetCore.Mvc;
using PainelGerencial.Domain;
using PainelGerencial.Domain.Vtex;
using RecursosCompartilhados.ViewModels.Vtex;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PainelGerencial.Services
{
    public interface IVtexServicos
    {
        Task<PedidoVtexViewModel> PedidoVtexUnico(string codigoVtex);
        Task<PedidoFila_Model> ConvertePedido(Feed_Model feed);
        Task<bool> IntegrarPedido(string token, string codigoExterno);
        Task<IEnumerable<Produto_Model>> ImagensProdutos(string dk);
        Task<StockKeepingUnit_Model> ImagensProdutosPorId(int skuId);
        Task<List<Vtex_Anuncios_Fotos_Model>> AnuncioFoto(Produto_Model produto);
        Task<Vtex_Anuncios_Fotos_Model> AnuncioFoto(StockKeepingUnit_Model produto, string dk);
        Task InserirProdutoImagemUnico(Vtex_Anuncios_Fotos_Model anuncio);
        Task InserirProdutoImagemLote(List<Vtex_Anuncios_Fotos_Model> listaAnuncios);
        Task<Vtex_Anuncios_Fotos_Model> IntegrarAnuncioUnico(string dk);
        Task<List<Vtex_Anuncios_Fotos_Model>> IntegrarAnuncios();
        Task<ProdutoVtexViewModel> ObterProdutoPorCodigoReferencia(string refId);
        Task<SkuByRefIdVtexViewModel> SkuPorRefId(string codigoExterno);
        Task<ProdutoVtexViewModel> CriarProduto(ProdutoVtexViewModel produto);
        Task<ProdutoVtexViewModel> AtualizarProduto(ProdutoVtexViewModel produto);
        Task<SkuByRefIdVtexViewModel> CriarSku(SkuByRefIdVtexViewModel produto);
        Task<string> AtualizarEan(int skuId, string ean);
        Task<List<StockKeepingUnitKit>> SkuListaKitPorParentId(int skuPai);
        Task<StockKeepingUnitKit> ExisteProduto(StockKeepingUnitKit kit);
        Task<StockKeepingUnitKit> CriarSkuKit(StockKeepingUnitKit kit);
        Task<SkuByRefIdVtexViewModel> AtualizarSku(SkuByRefIdVtexViewModel produto);

        //Acompanhamento Sellers.........................................................
        Task<ListSellersModel> Get_Lista_Sellers_Ativos(List<SellerParamModel> parametros);
        Task<dynamic> Get_PedidosSeller();
        Task<dynamic> GravaPedidos(PedidosSellerListModel ListPedidos);
        Task<dynamic> checaStatusPedidos();
    }
}

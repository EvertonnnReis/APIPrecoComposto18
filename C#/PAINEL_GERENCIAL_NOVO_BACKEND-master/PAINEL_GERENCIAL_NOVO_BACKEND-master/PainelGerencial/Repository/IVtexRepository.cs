using Domain.Vtex;
using PainelGerencial.Domain;
using PainelGerencial.Domain.Vtex;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PainelGerencial.Repository
{
    public interface IVtexRepository
    {
        public IEnumerable<PedidoFila_Model> GetPedidoFila(string codigoExterno);
        public int InserePedidoFila(PedidoFila_Model pedidoFila);
        public List<PedidoFila_Model> PedidosProntosParaManuseio();
        public int AtualizaPedidoIntegrado(string codigoExterno);
        bool ExisteSkuFila(string RefId);
        int InsereSkuFila(SkuByRefIdVtexViewModel produto);
        int AtualizaSkuFila(SkuByRefIdVtexViewModel produto);
        void GerarLogIntegracao(string codigoIdentificador, string tipoIdentificador, string mensagem, string metodo, int falha);
        int InserirProdutoImagem(Vtex_Anuncios_Fotos_Model produto);
        List<string> RetornaDks();
        Task<List<TbListSellersModel>> BuscaSellersSalvos(bool isActive);
        Task<dynamic> UpdateSellers(List<TbListSellersModel> listSellers);
        TbPedidosModel buscaUltimoPedido();
        Task<dynamic> NovosPedidos(List<TbPedidosModel> listPedidos);
        List<TbPedidosModel> buscaTodos();
        Task<dynamic> UpdatePedidos(List<TbPedidosModel> listPedidos);
    }
}
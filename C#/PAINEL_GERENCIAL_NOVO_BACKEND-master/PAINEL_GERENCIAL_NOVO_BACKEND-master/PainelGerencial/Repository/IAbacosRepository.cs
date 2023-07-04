using Domain.Abacos;
using Microsoft.AspNetCore.Mvc;
using PainelGerencial.Domain.Abacos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PainelGerencial.Repository
{
    public interface IAbacosRepository
    {
        public IEnumerable<dynamic> GetProsCod(string produto);
        public IEnumerable<dynamic> GetProdutosPorPedido(string pedido);
        public int CargaProduto(string codigoExterno);
        public IEnumerable<dynamic> PedidoStatus(string codigoExterno);
        public IEnumerable<TCOM_PEDSAI> PedidoIntegrado(string codigoExterno);
        public Task<IEnumerable<TCOM_PEDSAI>> PedidoNotaFiscal(string codigoExterno);
        Task<IEnumerable<dynamic>> ProdutoListaPreco(int listaPreco, string produtoCodigoExterno);
        int InserirProdutoAbacos(ProdutoAbacosViewModel produto);
        bool ExisteProdutoAbacos(string protocoloProduto);
        int InserirCategoriasDoSite(LinhaCategoriasDoSiteViewModel categoria, string codigoProduto);
        int InserirComponentesKit(LinhaComponentesKitViewModel componente, string codigoProduto);
        IEnumerable<dynamic> ListaPreco(int? codigoLista);
        string ConverterListaPrecos(int? codigoLista);
        IEnumerable<dynamic> ListaPrecoListar();
        Task<IEnumerable<TCOM_PROLIS>> ListaPrecoPorDKPai(string produtoPai);

        Task<IEnumerable<TCOM_COMPRO>> ComponentesKit(string codigoExternoKit);
        public Task<IEnumerable<dynamic>> BuscaRelatorioEstoqueMinimo(int filtro);
        public Task<IEnumerable<dynamic>> BuscaRelatorioProdutosMaisVendidos(int top, string dataInicial, string dataFinal);
    }
}
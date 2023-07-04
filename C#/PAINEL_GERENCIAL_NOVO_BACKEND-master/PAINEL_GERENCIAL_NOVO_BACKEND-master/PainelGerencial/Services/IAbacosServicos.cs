using AbacosWSERP;
using Domain.Abacos;
using Domain.Vtex;
using PainelGerencial.Domain.Vtex;
using PainelGerencial.Utils.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PainelGerencial.Services
{
    public interface IAbacosServicos
    {
        void AtivarProduto(ProdutoVtexViewModel produto, ProdutoAbacosViewModel produtoAbacos);
        Task<bool> AtualizarProduto(ProdutoVtexViewModel produto, ProdutoAbacosViewModel abacosViewModel);
        Task<bool> IntegracaoProdutos(ProdutoAbacosViewModel abacosViewModel);
        Task<int?> RecuperarProdutoIdSkuNovo(ProdutoAbacosViewModel abacosProduto);
        List<ProdutoAbacosViewModel> ConverterListaProduto(AbacosWSPlataforma.DadosProdutos[] produtos);
        Task<bool> AtualizarSku(SkuByRefIdVtexViewModel sku, ProdutoAbacosViewModel abacosViewModel);
        Task<StockKeepingUnitKit> GerarComponenteKit(LinhaComponentesKitViewModel componente, int? idSku);
        Task AtualizarKit(StockKeepingUnitKit kit);
        Task GerarKit(ProdutoAbacosViewModel produto, int? idSku);
        Task<bool> IntegracaoSkus(ProdutoAbacosViewModel abacosViewModel);
        Task<bool> ProdutoOuSku(ProdutoAbacosViewModel produtoVm);
        Task ConfirmarRecebimentoProduto(string protocolo);
        Task<bool> EnviarWsdlAbacos(DadosPreco[] dados);
        Task<bool> AlterarPrecoComponenteKit(DadosItemKit[] componente);
    }
}

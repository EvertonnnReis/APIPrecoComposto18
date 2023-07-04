using Domain.Abacos;
using Domain.Vtex;

namespace PainelGerencial.Utils
{
    public interface IProdutoConversao
    {
        ProdutoVtexViewModel ConverterProdutoVtex(ProdutoAbacosViewModel produto, ProdutoVtexViewModel objVtex);
    }
}

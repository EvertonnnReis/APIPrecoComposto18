using PainelGerencial.Domain.Fenix;
using System.Threading.Tasks;

namespace PainelGerencial.Services
{
    public interface IFenixServicos
    {
        Task<ProdutoComListaPrecoFenixViewModel> BuscarProduto(int listaPreco, string codigoExterno, bool somenteAtivos);
        Task<ProdutoKitComComponentesViewModel> BuscarProdutoKit(int listaPreco, string codigoExterno);
    }
}

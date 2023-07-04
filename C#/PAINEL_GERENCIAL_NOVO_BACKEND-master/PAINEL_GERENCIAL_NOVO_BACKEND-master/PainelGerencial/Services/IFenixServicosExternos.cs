using System.Threading.Tasks;
using PainelGerencial.Domain.Fenix;

namespace PainelGerencial.Services
{
    public interface IFenixServicosExternos
    {
        Task<ProdutoComListaPrecoFenixViewModel> ProdutoBuscar(int listaPreco, string codigoExterno);
    }
}

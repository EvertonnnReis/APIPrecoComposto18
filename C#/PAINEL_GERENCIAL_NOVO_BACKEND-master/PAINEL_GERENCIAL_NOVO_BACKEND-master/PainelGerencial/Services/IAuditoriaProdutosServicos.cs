using System.Threading.Tasks;

namespace PainelGerencial.Services
{
    public interface IAuditoriaProdutosServicos
    {
        Task<bool> Corrigido(int idProduto, string dk);
    }
}

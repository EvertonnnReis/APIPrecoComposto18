using PainelGerencial.Domain.Fenix;
using PainelGerencial.Domain.PrecificacaoCadastro;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PainelGerencial.Services
{
    public interface ISolicitacaoTemporariaServicos
    {
        void SalvarAlteracaoTemporaria(SolicitacaoTemporaria_Model solicitacao);
        Task<bool> VerificaPrecoZerado(string email);
        void ConfirmarSolicitacoesTemporarias(string email, int aplicarPrecoVendavel);
        double PrecoVenda(double valor);
    }
}

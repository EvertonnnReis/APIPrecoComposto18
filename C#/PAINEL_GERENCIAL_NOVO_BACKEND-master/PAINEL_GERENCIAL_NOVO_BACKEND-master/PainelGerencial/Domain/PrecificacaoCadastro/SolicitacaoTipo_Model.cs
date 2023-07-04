using System.Collections.Generic;

namespace PainelGerencial.Domain.PrecificacaoCadastro
{
    public class SolicitacaoTipo_Model
    {
        public int Codigo { get; set; }

        public string Descricao { get; set; }

        public virtual ICollection<Solicitacao_Model> Solicitacoes { get; set; }
    }
}

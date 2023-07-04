using System.Collections.Generic;

namespace PainelGerencial.Domain.PrecificacaoCadastro
{
    public class ParametrosAvaliarPreco_Model
    {
        public List<int> Codigos { get; set; }
        public bool Aprovado { get; set; }
        public string EmailAprovador { get; set; }
    }
}

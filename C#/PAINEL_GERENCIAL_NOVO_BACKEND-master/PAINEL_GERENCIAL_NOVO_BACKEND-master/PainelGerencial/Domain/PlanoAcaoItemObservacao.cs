using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PainelGerencial.Domain
{
    public class PlanoAcaoItemObservacao
    {
        public int? id { get; set; }
        public int plano_acao { get; set; }
        public int plano_acao_item { get; set; }
        public string descricao { get; set; }
        public DateTime? data_cadastro { get; set; } 
        public DateTime? data_alteracao { get; set; }
        public int usuario { get; set; }
        public string usuario_email { get; set; }
        public string usuario_nome { get; set; }
        public DateTime? data_prazo { get; set; }
        public int aprovado { get; set; }
        public int aprovado_usuario { get; set; }
        public string aprovado_usuario_nome { get; set; }
        public string aprovado_usuario_email { get; set; }
        public DateTime? data_aprovado { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PainelGerencial.Domain
{
    public class Agenda_Avaliacao_Model
    {
        public long? id { get; set; }
        public long agenda { get; set; }
        public int setor { get; set; }
        public int nota { get; set; }
        public string comentario { get; set; }
        public int usuario { get; set; }
        public string usuario_nome { get; set; }
        public string usuario_email { get; set; }
        public DateTime data_cadastro { get; set; }
        public DateTime data_alteracao { get; set; }
    }
}

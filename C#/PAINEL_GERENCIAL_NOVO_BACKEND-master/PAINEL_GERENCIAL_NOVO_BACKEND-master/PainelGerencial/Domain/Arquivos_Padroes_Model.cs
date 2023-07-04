using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PainelGerencial.Domain
{
    public class Arquivos_Padroes_Model
    {
        public int id { get; set; }
        public string nome { get; set; }
        public int setor { get; set; }
        public string nome_setor { get; set; }
        public string comentario { get; set; }
        public string arquivo { get; set; }
        public int usuario { get; set; }
        public string usuario_nome { get; set; }
        public string usuario_email { get; set; }
        public DateTime data_cadastro { get; set; }
        public DateTime data_alteracao { get; set; }
    }
}

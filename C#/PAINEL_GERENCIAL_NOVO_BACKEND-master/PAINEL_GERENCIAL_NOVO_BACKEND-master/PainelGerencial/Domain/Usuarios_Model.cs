using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PainelGerencial.Domain
{
    public class Usuarios_Model
    {
        public int id { get; set; }
        public string nome { get; set; }
        public string email { get; set; }
        public int setor { get; set; }
        public string setor_nome { get; set; }
        public DateTime data_cadastro { get; set; }
        public DateTime data_alteracao { get; set; }
        public int ativo { get; set; }
        public int? usuario { get; set; }
        public string usuario_nome { get; set; }
        public string usuario_email { get; set; }
    }
}

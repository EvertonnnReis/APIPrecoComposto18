using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PainelGerencial.Domain
{
    public class Painel_Model
    {
        public string quantidade { get; set; }
        public string valor_total { get; set; }
        public string ticket_medio { get; set; }
        public string pedido_mais_antigo { get; set; }
    }
    public class Painel_Model_quantidade
    {
        public string quantidade
        {
            get { return "0"; }
        }
    }

    public class Login
    {
        public string email { get; set; }
        public string senha { get; set; }
    }
}

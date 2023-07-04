using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PainelGerencial.Domain
{
    public class Total
    {
        public string quantidade { get; set; }
        public string valor_total { get; set; }
        public string ticket_medio { get; set; }
        public string porcentagem { get; set; }
    }

    public class Captacao_Model
    {
        public dynamic loja_virtual { get; set; }
        public dynamic market_place { get; set; }
        public dynamic market_place_connect { get; set; }
        public dynamic mercado_livre { get; set; }
        public dynamic pedidos_digitados { get; set; }

        public Total total { get; set; }
    }
}

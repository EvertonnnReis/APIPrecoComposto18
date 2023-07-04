using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PainelGerencial.Domain
{
    public class Cotacao_Model
    {
        public int codigoCotacao { get; set; }
        public int empresa { get; set; }
        public int cliente { get; set; }
        public string codigoExterno { get; set; }
        public double valor { get; set; }
        public double conversao { get; set; }
        public double encargos { get; set; }
        public double frete { get; set; }
        public double impostos { get; set; }
        public string obs { get; set; }
        public string vendedor { get; set; }
        public int codUsuario { get; set; }
        public int tipoCompra { get; set; }
        public int centro_custo { get; set; }
        public List<ItensCotacao_Model> itens { get; set; }
    }

    public class ItensCotacao_Model
    {
        public int codigoProduto { get; set; }
        public double quantidade { get; set; }
        public double precoBruto { get; set; }
        public double descontoTotal { get; set; }
        public double precoLiquido { get; set; }
    }
}

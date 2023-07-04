using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace PainelGerencial.Domain
{
    public class Perfil_Dos_Pedidos_Faturados
    {
        public int quantidade_mono { get; set; }
        public int quantidade_duplo { get; set; }
        public int quantidade_multiplo { get; set; }
        public int quantidade_total { get; set; }
        public string mono { get; set; }
        public string duplo { get; set; }
        public string multiplo { get; set; }
        public string Porcentagem_Grafico(float qtde, float qtde_total)
        {
            if (qtde_total == 0){
                return "0.0";
            }

			var resultado = ((qtde * 100) / qtde_total);

            return resultado.ToString("N1", CultureInfo.CreateSpecificCulture("en-US"));
        }
    }

    public class Perfil_Dos_Pedidos_Faturados_Model
    {
        public Perfil_Dos_Pedidos_Faturados perfil_dos_pedidos_faturados { get; set; }
    }
}

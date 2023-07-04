using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PainelGerencial.Domain
{
    public class Divergencia_Produto_Model
    {
        [Key]
        public int id { get; set; }
        public int id_Produto { get; set; }
        public string tipo { get; set; }
        public float? valor_pai { get; set; }
        public float? valor_kit { get; set; }
        public float? diferenca { get; set; }
    }
}

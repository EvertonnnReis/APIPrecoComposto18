using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PainelGerencial.Domain
{
    public class ProdutoFilho_Model
    {
        [Key]
        public int id { get; set; }
        public string dk { get; set; }
        public int? id_pai { get; set; }
        public string dk_pai { get; set; }
        public string nome { get; set; }
        public float? comprimento { get; set; }
        public float? largura { get; set; }
        public float? espessura { get; set; }
        public float peso { get; set; }
        public float? estoque { get; set; }
    }
}

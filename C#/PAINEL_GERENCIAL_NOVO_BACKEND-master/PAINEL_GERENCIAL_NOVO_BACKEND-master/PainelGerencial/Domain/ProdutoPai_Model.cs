using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PainelGerencial.Domain
{
    public class ProdutoPai_Model
    {
        [Key]
        public int id { get; set; }
        public string dk { get; set; }
        public string nome { get; set; }
        public float? comprimento { get; set; }
        public float? largura { get; set; }
        public float? espessura { get; set; }
        public double peso { get; set; }
        public int classe { get; set; }
        public string classe_nome { get; set; }
        public int estoque { get; set; }
        public int? ProdutoDiverGenteId { get; set; }
        public virtual ICollection<ProdutoDivergente_Model> ProdutoDivergente { get; set; }
    }
}

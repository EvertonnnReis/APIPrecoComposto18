using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PainelGerencial.Domain
{
    public class ProdutoDivergente_Model
    {
        [Key]
        public int id { get; set; }
        public string dk { get; set; }
        public int id_pai { get; set; }
        public string dk_pai { get; set; }
        public string nome { get; set; }
        public List<Divergencia_Produto_Model> divergencias { get; set; }
        public int? estoque { get; set; }
        public virtual ProdutoPai_Model ProdutoPai { get; set; }
    }
}

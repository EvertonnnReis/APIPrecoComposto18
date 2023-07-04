using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PainelGerencial.Domain
{
    public class Responsavel_Model
    {
        public int id { get; set; }
        public string nome { get; set; }
        public string email { get; set; }
        public bool? ticked { get; set; }
    }
}

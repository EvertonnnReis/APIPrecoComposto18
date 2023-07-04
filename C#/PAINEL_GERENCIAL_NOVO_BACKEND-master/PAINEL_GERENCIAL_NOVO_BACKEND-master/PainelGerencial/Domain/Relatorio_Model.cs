using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PainelGerencial.Domain
{
    public class Relatorio_Model
    {
        public string hora { get; set; }
        public string valor { get; set; }
        public int quantidade { get; set; }
        public float acumulado { get; set; }
    }

    public class ListaRelatorio
    {
        public List<Relatorio_Model> relatorio { get; set; }
    }


}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PainelGerencial.Domain
{
    public class PedidoStatus_Model
    {
        [Key]
        public int Codigo { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public string Icone { get; set; }
        public int? Minutos { get; set; }
        public DateTime? DataAtualizacaoRegistro { get; set; }
        public DateTime? DataCriacaoRegistro { get; set; }
    }
}

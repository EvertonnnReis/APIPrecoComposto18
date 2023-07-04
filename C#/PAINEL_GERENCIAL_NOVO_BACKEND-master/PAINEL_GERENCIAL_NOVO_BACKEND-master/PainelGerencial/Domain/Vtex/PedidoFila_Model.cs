using System;
using System.ComponentModel.DataAnnotations;

namespace PainelGerencial.Domain
{
    public class PedidoFila_Model
    {
        [Key]
        public int IdPedido { get; set; }
        public string Estado { get; set; }
        public string CodigoVtex { get; set; }
        public string CodigoExterno { get; set; }
        public DateTime? Integrado { get; set; }
        public DateTime? DataAtualizacaoRegistro { get; set; }
        public DateTime? DataCriacaoRegistro { get; set; }
    }
}

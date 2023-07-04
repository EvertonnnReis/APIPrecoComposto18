using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PainelGerencial.Domain
{
    public class PedidoFornecedor_Model
    {
        [Key]
        public string PedidoFornecedorCodigo { get; set; }
        public string PedidoCodigo { get; set; }
        public Guid FornecedorCodigo { get; set; }
        public int PedidoStatusCodigo { get; set; }
        public DateTime? DataConfirmacao { get; set; }
        public DateTime DataAtualizacaoRegistro { get; set; }
        public DateTime DataCriacaoRegistro { get; set; }
        public string? CodigoErpFornecedor { get; set; }
        public PedidoStatus_Model pedidoStatus { get; set; }
    }
}

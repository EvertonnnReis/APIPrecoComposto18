using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PainelGerencial.Domain
{
    // Tabela Dropshipping.dbo.Pedidos
    public class Pedido_Model
    {
        [Key]
        public string CodigoExterno { get; set; }
        public int PedidoStatusCodigo { get; set; }
        public int PedidoConnect { get; set; }
        public bool EmailEnviado { get; set; }
        public DateTime? DataConfirmacao { get; set; }
        public DateTime DataAtualizacaoRegistro { get; set; }
        public DateTime DataCriacaoRegistro { get; set; }
        public PedidoStatus_Model pedidoStatus { get; set; }
        public PedidoFornecedor_Model pedidoFornecedores { get; set; }
    }
}

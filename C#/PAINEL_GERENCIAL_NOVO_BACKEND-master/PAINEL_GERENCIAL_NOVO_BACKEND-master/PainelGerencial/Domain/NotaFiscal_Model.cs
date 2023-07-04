using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PainelGerencial.Domain
{
    public class NotaFiscal_Model
    {
        public string PedidoFornecedorCodigo { get; set; }
        public string Numero { get; set; }
        public string Rastreio { get; set; }
        public string Chave { get; set; }
        public DateTime DataEmissao { get; set; }
        public DateTime DataConfirmacao { get; set; }
        public DateTime DataAtualizacaoRegistro { get; set; }
        public DateTime DataCriacaoRegistro { get; set; }
        public bool Enviado { get; set; }
    }
}

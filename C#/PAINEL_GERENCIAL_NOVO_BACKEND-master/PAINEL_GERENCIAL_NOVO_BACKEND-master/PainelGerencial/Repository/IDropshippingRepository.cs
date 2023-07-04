using PainelGerencial.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PainelGerencial.Repository
{
    public interface IDropshippingRepository
    {
        public int AlteraPedidoCnpjCpf(string pedidoCodigoExterno);
        public Task<IEnumerable<Pedido_Model>> PedidoDropshipping(string pedidoCodigoExterno);
        public IEnumerable<NotaFiscal_Model> NotaFiscal(string pedidoCodigoExterno);
        public Task<IEnumerable<PedidoFornecedor_Model>> PedidoFornecedores(string pedidoCodigoExterno);
        public int PedidoFornecedoresAlteraStatus(string codigoExterno, int status);
        public IEnumerable<dynamic> PedidosFaturadosSemAndamento();
        public IEnumerable<PedidoFornecedor_Model> PedidosSemNotaFiscal();
        public int VoltaStatusPedidosSemNotaFiscal();
        public bool ExistePedido(string codigoExterno);
        public bool ExistePedidoFornecedores(string codigoExterno);
        public int InserePedido(Pedido_Model pedido);
        public int InserePedidoFornecedores(PedidoFornecedor_Model pedidoFornecedor);        
        public int InserePedidoFornecedorProdutos(string pedidoCodigoExterno);
    }
}

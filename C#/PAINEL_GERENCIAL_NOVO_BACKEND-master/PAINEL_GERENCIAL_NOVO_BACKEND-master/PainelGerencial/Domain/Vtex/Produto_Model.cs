using System.Collections.Generic;

namespace PainelGerencial.Domain.Vtex
{
    public class Produto_Model
    {
        public int productId { get; set; }
        public string productReferenceCode { get; set; }
        public List<ProdutoItem_Model> items { get; set; }
    }
}

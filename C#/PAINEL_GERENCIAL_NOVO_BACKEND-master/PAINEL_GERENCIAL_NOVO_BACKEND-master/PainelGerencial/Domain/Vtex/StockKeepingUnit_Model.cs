using System.Collections.Generic;

namespace PainelGerencial.Domain.Vtex
{
    public class StockKeepingUnit_Model
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string NameComplete { get; set; }
        public string ProductName { get; set; }
        public string ImageUrl { get; set; }
        public List<Imagem_Model> Images { get; set; }
    }
}

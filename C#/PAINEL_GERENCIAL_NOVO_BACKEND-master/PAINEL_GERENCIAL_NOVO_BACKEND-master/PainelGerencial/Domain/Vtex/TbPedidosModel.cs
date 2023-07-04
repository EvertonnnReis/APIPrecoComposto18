using System;

namespace PainelGerencial.Domain.Vtex
{
    public class TbPedidosModel
    {
        public string OrderID { get; set; }
        public DateTime CreationDate { get; set; }
        public string Status { get; set; }
        public string StatusDescription { get; set; }
        public DateTime LastChange { get; set; }
        public string SellerID { get; set; }
    }
}

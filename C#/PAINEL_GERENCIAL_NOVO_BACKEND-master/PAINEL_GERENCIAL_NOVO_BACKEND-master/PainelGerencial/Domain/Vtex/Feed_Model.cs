using System;

namespace PainelGerencial.Domain
{
    public class Feed_Model
    {
        public string? eventId { get; set; }
        public string handle { get; set; }
        public string domain { get; set; }
        public string state { get; set; }
        public string? lastState { get; set; }
        public string orderId { get; set; }
        public DateTime? lastChange { get; set; }
        public DateTime? currentChange { get; set; }
        public DateTime? availableDate { get; set; }
    }
}

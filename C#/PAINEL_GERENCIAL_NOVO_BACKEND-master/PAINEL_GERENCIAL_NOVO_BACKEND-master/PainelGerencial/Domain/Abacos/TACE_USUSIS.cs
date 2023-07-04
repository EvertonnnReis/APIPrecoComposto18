using System.ComponentModel.DataAnnotations;

namespace PainelGerencial.Domain.Abacos
{
    public class TACE_USUSIS
    {
        [Key]
        public int? USUS_COD { get; set; }
        public string USUS_NOM { get; set; }
    }
}

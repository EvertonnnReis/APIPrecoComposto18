using System.ComponentModel.DataAnnotations;

namespace PainelGerencial.Domain.Abacos
{
    public class TCOM_FAMPRO
    {
        [Key]
        public int FAMP_COD { get; set; }
        public string FAMP_NOM { get; set; }
    }
}

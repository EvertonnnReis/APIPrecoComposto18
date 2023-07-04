using System.ComponentModel.DataAnnotations;

namespace PainelGerencial.Domain.Abacos
{
    public class TCOM_CLAPRO
    {
        [Key]
        public int CLAP_COD { get; set; }
        public string CLAP_NOM { get; set; }
    }
}

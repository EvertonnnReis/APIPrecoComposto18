using System.ComponentModel.DataAnnotations;

namespace PainelGerencial.Domain.Abacos
{
    public class TGEN_NFEENV
    {
        [Key]
        public int NFEL_COD { get; set; }
        public int NOTF_COD { get; set; }
        public string NFEE_CHR_TIP { get; set; }
        public int NFEE_STA_CONSTA { get; set; }
        public string NFEE_TXT_ENVXML { get; set; }
    }
}

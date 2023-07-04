using System.ComponentModel.DataAnnotations;

namespace PainelGerencial.Domain.Abacos
{
    public class TCOM_MARPRO
    {
        [Key]
        public int MARP_COD { get; set; }
        public string MARP_NOM { get; set; }
    }
}

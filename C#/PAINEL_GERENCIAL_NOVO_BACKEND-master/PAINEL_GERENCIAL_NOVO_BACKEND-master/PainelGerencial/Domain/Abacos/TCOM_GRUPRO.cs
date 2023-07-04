using System.ComponentModel.DataAnnotations;

namespace PainelGerencial.Domain.Abacos
{
    public class TCOM_GRUPRO
    {
        [Key]
        public int GRUP_COD { get; set; }
        public string GRUP_NOM { get; set; }
    }
}

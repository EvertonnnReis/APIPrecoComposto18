using System.ComponentModel.DataAnnotations;

namespace PainelGerencial.Domain.Abacos
{
    public class TGEN_UNIMON
    {
        [Key]
        public int UNIM_COD { get; set; }
        public string UNIM_NOM { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace PainelGerencial.Domain.Abacos
{
    public class TCOM_LISPRE
    {
        [Key]
        public int LISP_COD { get; set; }
        public string LISP_NOM { get; set; }
        public int? UNIM_COD { get; set; }
        public TGEN_UNIMON TGEN_UNIMON { get; set; }
    }
}

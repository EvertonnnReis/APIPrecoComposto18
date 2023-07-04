using System.ComponentModel.DataAnnotations;

namespace PainelGerencial.Domain.Abacos
{
    public class TGEN_NOTFIS
    {
        [Key]
        public int NOTF_COD { get; set; }
        public int PEDS_COD { get; set; }
        public TGEN_NFEENV? TGEN_NFEENV { get; set; }
    }
}

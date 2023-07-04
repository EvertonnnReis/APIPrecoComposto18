using System.ComponentModel.DataAnnotations;

namespace PainelGerencial.Domain.Abacos
{
    public class TEST_SALPRO
    {
        [Key]
        public int SALP_COD { get; set; }
        public int PROS_COD { get; set; }
        public int ALMO_COD { get; set; }
        public float SALP_VAL_CUSBRU { get; set; }
    }
}

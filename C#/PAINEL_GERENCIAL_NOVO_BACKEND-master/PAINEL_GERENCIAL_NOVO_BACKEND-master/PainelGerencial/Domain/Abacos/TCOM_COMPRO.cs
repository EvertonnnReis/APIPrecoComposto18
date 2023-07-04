using System.ComponentModel.DataAnnotations;

namespace PainelGerencial.Domain.Abacos
{
    public class TCOM_COMPRO
    {
        [Key]
        public int COMP_COD { get; set; }
        public int PROS_COD { get; set; }
        public TCOM_PROSER TCOM_PROSER { get; set; }
        public int COMP_COD_PRO { get; set; }
        public TCOM_PROSER ProdutoComponente { get; set; }
        public float COMP_QTF { get; set; }
    }
}

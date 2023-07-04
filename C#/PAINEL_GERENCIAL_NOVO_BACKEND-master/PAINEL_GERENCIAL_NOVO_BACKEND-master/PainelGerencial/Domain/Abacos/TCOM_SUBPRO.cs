using AbacosWSERP;
using System.ComponentModel.DataAnnotations;

namespace PainelGerencial.Domain.Abacos
{
    public class TCOM_SUBPRO
    {
        [Key]
        public int SUBP_COD { get; set; }
        public string SUBP_NOM { get; set; }
        public int? GRUP_COD { get; set; }
        public TCOM_GRUPRO TCOM_GRUPRO { get; set; }
    }
}

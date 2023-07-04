using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PainelGerencial.Domain.Abacos
{
    public class TCOM_PROSER
    {
        [Key]
        public int PROS_COD { get; set; }
        public string PROS_EXT_COD { get; set; }
        public string PROS_NOM { get; set; }
        public string? PROS_CFB { get; set; }
        public int? FAMP_COD { get; set; }
        public TCOM_FAMPRO TCOM_FAMPRO { get; set; }
        public int? SUBP_COD { get; set; }
        public TCOM_SUBPRO TCOM_SUBPRO { get; set; }
        public int? MARP_COD { get; set; }
        public TCOM_MARPRO TCOM_MARPRO { get; set; }
        public int? CLAP_COD { get; set; }
        public TCOM_CLAPRO TCOM_CLAPRO { get; set; }
        public IList<TCOM_COMPRO>? listaComponentes { get; set; }
        public IList<TCOM_PROLIS>? TCOM_PROLIS { get; set; }
        public IList<TEST_SALPRO>? TEST_SALPRO { get; set; }
    }
}

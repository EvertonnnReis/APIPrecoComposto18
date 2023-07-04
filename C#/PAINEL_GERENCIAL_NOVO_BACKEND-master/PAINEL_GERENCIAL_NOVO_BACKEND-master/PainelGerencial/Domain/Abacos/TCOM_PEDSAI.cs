using System;
using System.ComponentModel.DataAnnotations;

namespace PainelGerencial.Domain.Abacos
{
    public class TCOM_PEDSAI
    {
        [Key]
        public int PEDS_COD { get; set; }
        public string PEDS_EXT_COD { get; set; }
        public DateTime? PEDS_DAT_CAD { get; set; }
        public int COME_COD { get; set; }
        public int STAP_COD { get; set; }
        public int UNIN_COD { get; set; }
        public TGEN_NOTFIS? TGEN_NOTFIS { get; set; }
    }
}

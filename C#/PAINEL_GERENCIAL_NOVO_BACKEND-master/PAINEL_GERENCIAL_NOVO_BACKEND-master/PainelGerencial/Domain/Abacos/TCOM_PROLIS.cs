using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PainelGerencial.Domain.Abacos
{
    public class TCOM_PROLIS
    {
        [Key]
        public int PROL_COD { get; set; }
        public int LISP_COD { get; set; }
        public TCOM_LISPRE TCOM_LISPRE { get; set; }
        public int PROS_COD { get; set; }
        public TCOM_PROSER TCOM_PROSER { get; set; }
        public float PROL_VAL_PRE { get; set; }
        public float PROL_VAL_PREPRO { get; set; }
        public DateTime? PROL_DAT_INIPRO { get; set; }
        public DateTime? PROL_DAT_FIMPRO { get; set; }
        public DateTime PROL_DAT_CAD { get; set; }
        public DateTime? PROL_DAT_ALT { get; set; }
        public int? USUS_COD { get; set; }
        public TACE_USUSIS TACE_USUSIS { get; set; }
        public float? PROL_PER { get; set; }

        // Calculado
        public float? CustoBruto { get; set; }
    }
}

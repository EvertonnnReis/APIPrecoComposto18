namespace PainelGerencial.Domain.Abacos
{
    public class TCOM_PRCCOM
    {
        public int PRCC_COD { get; set; }
        public int? PROS_COD { get; set; }
        public TCOM_PROSER? TCOM_PROSER { get; set; }
        public float? PRCC_VAL_PRETAB { get; set; }
    }
}

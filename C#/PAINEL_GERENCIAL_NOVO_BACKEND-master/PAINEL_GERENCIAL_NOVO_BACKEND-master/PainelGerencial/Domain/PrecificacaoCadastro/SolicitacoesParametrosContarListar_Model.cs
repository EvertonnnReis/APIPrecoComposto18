namespace PainelGerencial.Domain.PrecificacaoCadastro
{
    public class SolicitacoesParametrosContarListar_Model
    {
        public string ProdutoCodigoExternoPai { get; set; }

        public int? SolicitacaoTipoCodigo { get; set; }

        public int? SolicitacaoStatusCodigo { get; set; }

        public int? ProdutoListaPrecoCodigo { get; set; }

        public int? ProdutoGrupoCodigo { get; set; }

        public string? ProdutoGrupoCodigos { get; set; }

        public bool? SomentePrecosReduzidos { get; set; }

        public string SolicitanteResponsavelEmail { get; set; }

        public string? AprovadorResponsavelEmail { get; set; }

        public string? DataInicial { get; set; }

        public string? DataFinal { get; set; }

        public bool? SomentePais { get; set; }
    }
}

namespace PainelGerencial.Domain.PrecificacaoCadastro
{
    public class SolicitacaoTemporaria_Model
    {
        public int Codigo { get; set; }
        public string ProdutoCodigoExterno { get; set; }
        public string ProdutoCodigoExternoPai { get; set; }
        public string ProdutoNome { get; set; }
        public string ProdutoNomePai { get; set; }
        public string Email { get; set; }
        public bool ProdutoPai { get; set; }
        public bool Kit { get; set; }
        public int Grupo { get; set; }
        public double ProdutoCusto { get; set; }
        public double ValorDe { get; set; }
        public double ValorPor { get; set; }
        public double ValorDefinido { get; set; }
        public double PercentualAjuste { get; set; }
        public int TabelaPreco { get; set; }
        public double ValorMinimo { get; set; }
        public double? ValorMinimoCalculado { get; set; }
        public int TipoKit { get; set; }
        public int? Observacao { get; set; }
        public bool Alterado { get; set; }
    }
}

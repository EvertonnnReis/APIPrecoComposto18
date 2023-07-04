namespace PainelGerencial.Domain.PrecificacaoCadastro
{
    public class AvaliacaoSolicitacao_Model
    {
        public int Codigo { get; set; }
        public string AprovadorResponsavelEmail { get; set; }
        public string Observacao { get; set; }
        public string ProdutoNomeReduzidoNovo { get; set; }
        public double? ProdutoPrecoPromocionalNovo { get; set; }
        public int SolicitacaoStatusCodigo { get; set; }
    }
}

using System;

namespace PainelGerencial.Domain.PrecificacaoCadastro
{
    public class Precificacao_Model
    {
        public int Id { get; set; }
        public string? ProdutoCodigoExterno { get; set; }
        public string? ProdutoCodigoExternoPai { get; set; }
        public string? ProdutoNome { get; set; }
        public string? SolicitanteResponsavelEmail { get; set; }
        public string? AprovadorResponsavelEmail { get; set; }
        public string? Observacao { get; set; }
        public bool? Aprovado { get; set; }
        public DateTime? DataConfirmacao { get; set; }
        public int? ProdutoListaPrecoCodigo { get; set; }
        public ListaPrecos_Model? ListaPreco { get; set; }
        public decimal? ProdutoPrecoTabela { get; set; }
        public decimal? ProdutoPrecoPromocional { get; set; }
        public decimal? ProdutoPrecoTabelaNovo { get; set; }
        public decimal? ProdutoPrecoPromocionalNovo { get; set; }
        public DateTime DataAtualizacaoRegistro { get; set; }
        public DateTime DataCriacaoRegistro { get; set; }
    }
}

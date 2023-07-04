using System;
using System.ComponentModel.DataAnnotations;

namespace PainelGerencial.Domain.PrecificacaoCadastro
{
    public class SolicitacoesKitComponentes_Model
    {
        [Key]
        public int Codigo { get; set; }
        public string ProdutoCodigoExterno { get; set; }
        public string ProdutoCodigoExternoPai { get; set; }
        public string? ProdutoNome { get; set; }
        public string? ProdutoNomePai { get; set; }
        public int? ProdutoListaPrecoCodigo { get; set; }
        public float? ProdutoPrecoTabela { get; set; }
        public float? ProdutoPrecoPromocional { get; set; }
        public float? ProdutoPrecoTabelaNovo { get; set; }
        public float? ProdutoPrecoPromocionalNovo { get; set; }
        public DateTime DataAtualizacaoRegistro { get; set; }
        public DateTime DataCriacaoRegistro { get; set; }
        public int SolicitacaoKit_Codigo { get; set; }
        public float Quantidade { get; set; }
        public float? ProdutoPrecoTabelaUnitarioNovo { get; set; }
        public float? ProdutoPrecoPromocionalUnitarioNovo { get; set; }
    }
}

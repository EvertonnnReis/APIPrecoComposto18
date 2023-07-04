using System;
using System.Collections.Generic;

namespace PainelGerencial.Domain.PrecificacaoCadastro
{
    public class Solicitacao_Model
    {
        public int Codigo { get; set; }
        public int SolicitacaoTipoCodigo { get; set; }
        public virtual SolicitacaoTipo_Model SolicitacaoTipo { get; set; }
        public string ProdutoCodigoExterno { get; set; }
        public string ProdutoCodigoExternoPai { get; set; }
        public string ProdutoNome { get; set; }
        public string ProdutoNomePai { get; set; }
        public int ProdutoGrupoCodigo { get; set; }
        public string SolicitanteResponsavelEmail { get; set; }
        public string AprovadorResponsavelEmail { get; set; }
        public string Observacao { get; set; }
        public int SolicitacaoStatusCodigo { get; set; }
        public virtual SolicitacaoStatus_Model SolicitacaoStatus { get; set; }
        //Campos opcionais que dependem de cada tipo de variação
        public string ProdutoNomeNovo { get; set; }
        public string ProdutoNomeReduzidoNovo { get; set; }
        public string ProdutoDescricaoNova { get; set; }
        public int? ProdutoListaPrecoCodigo { get; set; }
        public double? ProdutoPrecoTabela { get; set; }
        public double? ProdutoPrecoPromocional { get; set; }
        public double? ProdutoPrecoTabelaNovo { get; set; }
        public double? ProdutoPrecoPromocionalNovo { get; set; }
        public string ProdutoClasse { get; set; }
        public string ProdutoClasseNova { get; set; }
        public DateTime DataAtualizacaoRegistro { get; set; }
        public DateTime DataCriacaoRegistro { get; set; }

        public void AprovarSolicitacao(int produtoGrupoCodigo, string aprovadorResponsavelEmail)
        {
            SolicitacaoStatusCodigo = 2;
            ProdutoGrupoCodigo = produtoGrupoCodigo;
            Observacao = $"A alteração de classe foi aprovada por : " + aprovadorResponsavelEmail;
        }

        public void IncluirNaPendencia(int produtoGrupoCodigo, string aprovadorResponsavelEmail)
        {
            SolicitacaoStatusCodigo = 1;
            ProdutoGrupoCodigo = produtoGrupoCodigo;
            Observacao = $"A alteração de classe foi pré aprovada por : " + aprovadorResponsavelEmail;
        }
    }

    public class RequisicaoSolicitacaoComKit
    {
        public string email { get; set; }
        public string codigoPai { get; set; }
        public List<int> tabela { get; set; }
        public int tipoKit { get; set; }
    }
}

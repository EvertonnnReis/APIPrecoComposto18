using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PainelGerencial.Domain.PrecificacaoCadastro
{
    public class SolicitacoesKit_Model
    {
        [Key]
        public int Codigo { get; set; }
        public string ProdutoCodigoExterno { get; set; }
        public string ProdutoCodigoExternoPai { get; set; }
        public string? ProdutoNome { get; set; }
        public string? ProdutoNomePai { get; set; }
        public int ProdutoGrupoCodigo { get; set; }
        public string SolicitanteResponsavelEmail { get; set; }
        public string? AprovadorResponsavelEmail { get; set; }
        public int SolicitacaoStatusCodigo { get; set; }
        public bool Confirmado { get; set; }
        public int? ProdutoListaPrecoCodigo { get; set; }
        public string? ConfirmacaoEmail { get; set; }
        public DateTime DataAtualizacaoRegistro { get; set; }
        public DateTime DataCriacaoRegistro { get; set; }
        public bool? Aprovado { get; set; }
        public string? Observacao { get; set; }
        public List<SolicitacoesKitComponentes_Model>? SolicitacoesKitComponentes { get; set; }
    }
}

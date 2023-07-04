using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace PainelGerencial.Domain.Fenix
{
    public class ProdutoFenixViewModel
    {
        public int Codigo { get; set; }
        public string CodigoExterno { get; set; }
        public int CodigoPai { get; set; }
        public string CodigoExternoPai { get; set; }
        public string CodigoFabricacao { get; set; }
        public string Ean { get; set; }
        public string Nome { get; set; }
        public string NomePai { get; set; }
        public string NomeReduzido { get; set; }
        public string NomeComercial { get; set; }
        public string Descricao { get; set; }
        public string Kit { get; set; }
        public double? EstoqueAtual { get; set; }
        public double? EstoqueVirtual { get; set; }
        public double? CustoBruto { get; set; }
        public double? CustoLiquido { get; set; }
        public double? PrecoCusto { get; set; }
        public DateTime? DataUltimaCompra { get; set; }
        public DateTime? DataFim { get; set; }
        public DateTime? DataCadastro { get; set; }
        public DateTime? DataAtualizacao { get; set; }
        public ProdutoDimensoesFenixViewModel Dimensoes { get; set; }
        public ProdutoNomeavelFenixViewModel Classe { get; set; }
        public ProdutoNomeavelFenixViewModel Marca { get; set; }
        public ProdutoNomeavelFenixViewModel Grupo { get; set; }
        public ProdutoNomeavelFenixViewModel Subgrupo { get; set; }
    }

    public class ProdutoDimensoesFenixViewModel
    {
        public double? Peso { get; set; }
        public double? Comprimento { get; set; }
        public double? Largura { get; set; }
        public double? Altura { get; set; }
        public double? Volume { get; set; }
    }

    public class ProdutoNomeavelFenixViewModel
    {
        public int? Codigo { get; set; }
        public string Nome { get; set; }
    }

    [JsonConverter(typeof(GroupingConverter))]
    public class ProdutoComListaPrecoFenixViewModel : ProdutoFenixViewModel
    {
        public IEnumerable<ProdutoListaPrecoComValorFenixViewModel> ListaPrecos { get; set; }
    }

    public class ProdutoTiposPrecificacaoViewModel
    {
        public string Nome { get; set; }
        public string Codigo { get; set; }
        public bool PossuiKits { get; set; }
        public bool PossuiNaoKits { get; set; }
        public bool Kit { get; set; }
    }

    public class AbacosProdutoResumoViewModel
    {
        public string Codigo { get; set; }
        public string CodigoExterno { get; set; }
        public string CodigoPai { get; set; }
        public string CodigoExternoPai { get; set; }
        public string Titulo { get; set; }
        public int? ClasseCodigo { get; set; }
        public string ClasseNome { get; set; }
        public int? MarcaCodigo { get; set; }
        public string MarcaNome { get; set; }
        public int? GrupoCodigo { get; set; }
        public string GrupoNome { get; set; }
        public int? SubgrupoCodigo { get; set; }
        public string SubgrupoNome { get; set; }
        public DateTime? DataCadastro { get; set; }
        public DateTime? DataAtualizacao { get; set; }
    }
}

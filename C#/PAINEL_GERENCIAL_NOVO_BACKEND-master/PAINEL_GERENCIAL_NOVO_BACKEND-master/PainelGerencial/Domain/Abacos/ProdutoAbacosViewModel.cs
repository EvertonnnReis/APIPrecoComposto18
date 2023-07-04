using System.Collections.Generic;
using Newtonsoft.Json;

namespace Domain.Abacos
{
    public class LinhaDescritorSimplesViewModel
    {
        public int CodigoInterno { get; set; }
        public int Numero { get; set; }
        public string Descricao { get; set; }
    }

    public class ResultadoOperacaoViewModel
    {
        public int Codigo { get; set; }
        public string Descricao { get; set; }
        //public TipoDeResultadoEnum Tipo { get; set; }
        public string ExceptionMessage { get; set; }
        public string ExceptionClassName { get; set; }
        public string ExceptionStackTrace { get; set; }
    }

    public class DescritorSimplesViewModel
    {
        [JsonProperty("Rows")]
        public List<LinhaDescritorSimplesViewModel> Linhas { get; set; }
        public string VersaoWebService { get; set; }
        public ResultadoOperacaoViewModel ResultadoOperacao { get; set; }
    }

    public class LinhaDescritroPreDefinidoViewModel
    {
        public int CodigoInterno { get; set; }
        public int Numero { get; set; }
        public string Descricao { get; set; }
        public int GrupoCodigo { get; set; }
        public string GrupoNome { get; set; }
    }

    public class DescritorPreDefinidoViewModel
    {
        [JsonProperty("Rows")]
        public List<LinhaDescritroPreDefinidoViewModel> Linhas { get; set; }
        public string VersaoWebService { get; set; }
        public ResultadoOperacaoViewModel ResultadoOperacao { get; set; }
    }

    public class LinhaAtributosEstendidosViewModel
    {
        public int CodigoAtributo { get; set; }
        public string NomeAtributo { get; set; }
        public string NomeElemento { get; set; }
        public string TipoDado { get; set; }
        public string CampoValor { get; set; }
        public int Ordenacao { get; set; }
    }

    public class AtributosEstendidosViewModel
    {
        [JsonProperty("Rows")]
        public List<LinhaAtributosEstendidosViewModel> Linhas { get; set; }
        public string VersaoWebService { get; set; }
        public ResultadoOperacaoViewModel ResultadoOperacao { get; set; }
    }

    public class LinhaCaracteristicasComplementaresViewModel
    {
        public int CodigoInterno { get; set; }
        public int TipoCodigo { get; set; }
        public string TipoNome { get; set; }
        public int TipoGrupoCodigo { get; set; }
        public string TipoGrupoNome { get; set; }
        public string Texto { get; set; }
        public int NrConfigInterface { get; set; }
    }

    public class CaracteristicasComplementaresViewModel
    {
        [JsonProperty("Rows")]
        public List<LinhaCaracteristicasComplementaresViewModel> Linhas { get; set; }
        public string VersaoWebService { get; set; }
        public ResultadoOperacaoViewModel ResultadoOperacao { get; set; }
    }

    public class LinhaProdutosSubstitutosViewModel
    {
        public string CodigoSubstituto { get; set; }
        public string CodigoBarrasProdutoSubstituto { get; set; }
        public string IdentificadorProdutoSubstituto { get; set; }
    }

    public class ProdutosSubstitutosViewModel
    {
        [JsonProperty("Rows")]
        public List<LinhaProdutosSubstitutosViewModel> Linhas { get; set; }
        public string VersaoWebService { get; set; }
        public ResultadoOperacaoViewModel ResultadoOperacao { get; set; }
    }

    public class LinhaCategoriasDoSiteViewModel
    {
        public int CodigoCategoria { get; set; }
        public int CodigoCategoriaPai { get; set; }
        public string CodigoExternoCategoria { get; set; }
    }

    public class CategoriasDoSiteViewModel
    {
        [JsonProperty("Rows")]
        public List<LinhaCategoriasDoSiteViewModel> Linhas { get; set; }
        public string VersaoWebService { get; set; }
        public ResultadoOperacaoViewModel ResultadoOperacao { get; set; }
    }

    public class LinhaProdutosPersonalizacaoViewModel
    {
        public string ClassificacaoPersonalizacao { get; set; }
        public string CodigoProdutoPersonalizacao { get; set; }
        public string CodigoProdutoPersonalizacaoPai { get; set; }
        public string ProdutoPersonalizacaoLigacao { get; set; }
    }

    public class ProdutosPersonalizacaoViewModel
    {
        [JsonProperty("Rows")]
        public List<LinhaProdutosPersonalizacaoViewModel> Linhas { get; set; }
        public string VersaoWebService { get; set; }
        public ResultadoOperacaoViewModel ResultadoOperacao { get; set; }
    }

    public class LinhaComponentesKitViewModel
    {
        public string CodigoProdutoComponente { get; set; }
        public double Quantidade { get; set; }
        public string DescritorPreDefinido1 { get; set; }
        public string DescritorPreDefinido2 { get; set; }
        public string DescritorPreDefinido3 { get; set; }
        public string CodigoDescritorPreDefinido1 { get; set; }
        public string CodigoDescritorPreDefinido2 { get; set; }
        public string CodigoDescritorPreDefinido3 { get; set; }
        public string DescritorSimples1 { get; set; }
        public string DescritorSimples2 { get; set; }
        public string DescritorSimples3 { get; set; }
        public double Preco { get; set; }
        public double PrecoPromocional { get; set; }
        public string DataInicioPromocao { get; set; }
        public string DataTerminoPromocao { get; set; }
    }

    public class ComponentesKitViewModel
    {
        [JsonProperty("Rows")]
        public List<LinhaComponentesKitViewModel> Linhas { get; set; }
        public string VersaoWebService { get; set; }
        public ResultadoOperacaoViewModel ResultadoOperacao { get; set; }
    }

    public class LinhaProdutosAssociadosViewModel
    {
        public string CodigoProdutoAbacos { get; set; }
        public string CodigoProduto { get; set; }
        public string CodigoBarras { get; set; }
        public string OrdemAssociacao { get; set; }
        public string TipoAssociacao { get; set; }
        public string TipoAssociacaoNome { get; set; }
        public string TipoNome { get; set; }
        public string TipoCategoria { get; set; }
        public string TipoCategoriaNome { get; set; }
    }

    public class ProdutosAssociadosViewModel
    {
        [JsonProperty("Rows")]
        public List<LinhaProdutosAssociadosViewModel> Linhas { get; set; }
        public string VersaoWebService { get; set; }
        public ResultadoOperacaoViewModel ResultadoOperacao { get; set; }
    }

    public class LinhaDadosColecaoViewModel
    {
        public string NomeColecao { get; set; }
    }

    public class DadosColecaoViewModel
    {
        [JsonProperty("Rows")]
        public List<LinhaDadosColecaoViewModel> Linhas { get; set; }
        public string VersaoWebService { get; set; }
        public ResultadoOperacaoViewModel ResultadoOperacao { get; set; }
    }

    public class LinhaDadosContribuidorViewModel
    {
        public string NomeContribuidor { get; set; }
        public string NomeFuncao { get; set; }
        public string PrincipalFuncao { get; set; }
    }

    public class DadosContribuidorViewModel
    {
        [JsonProperty("Rows")]
        public List<LinhaDadosContribuidorViewModel> Linhas { get; set; }
        public string VersaoWebService { get; set; }
        public ResultadoOperacaoViewModel ResultadoOperacao { get; set; }
    }

    public class LinhaDadosAcompanhamentoViewModel
    {
        public string NomeAcompanhamento { get; set; }
    }

    public class DadosAcompanhamentoViewModel
    {
        [JsonProperty("Rows")]
        public List<LinhaDadosAcompanhamentoViewModel> Linhas { get; set; }
        public string VersaoWebService { get; set; }
        public ResultadoOperacaoViewModel ResultadoOperacao { get; set; }
    }

    public class LinhaDadosLivrosViewModel
    {
        public string NomeIdioma { get; set; }
        public string NomePais { get; set; }
        public string DataPublicacao { get; set; }
        public int NumeroPaginas { get; set; }
        public int Anopublicacao { get; set; }
        public string Isbn10 { get; set; }
        public int NumeroVolume { get; set; }
        public string ImpressaoSobDemanda { get; set; }
        public string ComplementoEdicao { get; set; }
        public string NomeEncadernacao { get; set; }
        public int NumeroEdicao { get; set; }
        public int CodigoEdicaoAnterior { get; set; }
        public string EdicaoAnterior { get; set; }
        public string SituacaoIndisponivel { get; set; }
        public DadosColecaoViewModel DadosColecao { get; set; }
        public DadosContribuidorViewModel DadosContribuidor { get; set; }
        public DadosAcompanhamentoViewModel DadosAcompanhamento { get; set; }
    }

    public class DadosLivrosViewModel
    {
        [JsonProperty("Rows")]
        public List<LinhaDadosLivrosViewModel> Linhas { get; set; }
        public string VersaoWebService { get; set; }
        public ResultadoOperacaoViewModel ResultadoOperacao { get; set; }
    }

    public class LinhaDadosImagemProdutoViewModel
    {
        public string FinalidadeImagem { get; set; }

        public string Repositorio { get; set; }
    }

    public class DadosImagemProdutoViewModel
    {
        [JsonProperty("Rows")]
        public List<LinhaDadosImagemProdutoViewModel> Linhas { get; set; }
        public string VersaoWebService { get; set; }
        public ResultadoOperacaoViewModel ResultadoOperacao { get; set; }
    }

    public class LinhaOpcoesClassificacoesComplementaresViewModel
    {
        public string CodigoOpcaoExterno { get; set; }
        public string CodigoOpcaoAbacos { get; set; }
        public string CodigoOpcaoPaiAbacos { get; set; }
        public string DescricaoOpcao { get; set; }
        public ResultadoOperacaoViewModel ResultadoOperacao { get; set; }
    }

    public class OpcoesClassificacoesComplementaresViewModel
    {
        [JsonProperty("Rows")]
        public List<LinhaOpcoesClassificacoesComplementaresViewModel> Linhas { get; set; }
        public string VersaoWebService { get; set; }
        public ResultadoOperacaoViewModel ResultadoOperacao { get; set; }
    }

    public class LinhaClassificacoesComplementaresViewModel
    {
        public string CodigoGrupoExterno { get; set; }
        public string CodigoGrupoAbacos { get; set; }
        public string CodigoGrupoPaiAbacos { get; set; }
        public string DescricaoGrupo { get; set; }
        public string TipoGrupo { get; set; }
        public string DescricaoOpcao { get; set; }
        public OpcoesClassificacoesComplementaresViewModel OpcoesClassificacoesComplementares { get; set; }
        public ResultadoOperacaoViewModel ResultadoOperacao { get; set; }
    }

    public class ClassificacoesComplementaresViewModel
    {
        [JsonProperty("Rows")]
        public List<LinhaClassificacoesComplementaresViewModel> Linhas { get; set; }
        public string VersaoWebService { get; set; }
        public ResultadoOperacaoViewModel ResultadoOperacao { get; set; }
    }

    public class LinhaTransportadorasHabilitadasViewModel
    {
        public string CodigoTransportadora { get; set; }
        public string NomeTransportadora { get; set; }
        public string CnpjTransportadora { get; set; }
    }

    public class TransportadorasHabilitadasViewModel
    {
        [JsonProperty("Rows")]
        public List<LinhaTransportadorasHabilitadasViewModel> Linhas { get; set; }
        public string VersaoWebService { get; set; }
        public ResultadoOperacaoViewModel ResultadoOperacao { get; set; }
    }

    public class LinhaProdutosAtributosEstendidosViewModel
    {
        public string Acao { get; set; }
        public string NomeAtributo { get; set; }
        public string ValorAtributo { get; set; }

    }

    public class ProdutoAbacosViewModel
    {
        public string ProtocoloProduto { get; set; }
        //public TAcaoIntegracaoEnum Acao { get; set; }
        public int CodigoProdutoAbacos { get; set; }
        public string CodigoProduto { get; set; }
        public string CodigoProdutoPai { get; set; }
        public string CodigoBarras { get; set; }
        public string CodigoBarrasPai { get; set; }
        public string TipoProduto { get; set; }
        public string NomeProduto { get; set; }
        public string NomeProdutoReduzido { get; set; }
        public string Descricao { get; set; }
        public int CodigoMarca { get; set; }
        public string CodigoFabricante { get; set; }
        public int CodigoClasse { get; set; }
        public string CodigoExternoClasse { get; set; }
        public int CodigoFamilia { get; set; }
        public string CodigoExternoFamilia { get; set; }
        public int CodigoGrupo { get; set; }
        public int CodigoSubGrupo { get; set; }
        public double Peso { get; set; }
        public double Largura { get; set; }
        public double Comprimento { get; set; }
        public double Profundidade { get; set; }
        public double Espessura { get; set; }
        public double Altura { get; set; }
        public int QtdePorEmbalagem { get; set; }
        public double QtdeMinimaEstoque { get; set; }
        public double QtdeMaximaEstoque { get; set; }
        public string UnidadeMedidaNome { get; set; }
        public string UnidadeMedidaAbrev { get; set; }
        public bool UnidadeMedidaQuilo { get; set; }
        public bool ProdutoTemFilhos { get; set; }
        public int PrazoEntregaDias { get; set; }
        public string CampoCfg001 { get; set; }
        public string CampoCfg002 { get; set; }
        public string CampoCfg003 { get; set; }
        public string CampoCfg004 { get; set; }
        public string CampoCfg005 { get; set; }
        public string CampoCfg006 { get; set; }
        public string CampoCfg007 { get; set; }
        public string CampoCfg008 { get; set; }
        public string CampoCfg009 { get; set; }
        public string CampoCfg010 { get; set; }
        public string CampoCfg011 { get; set; }
        public string CampoCfg012 { get; set; }
        public string CampoCfg013 { get; set; }
        public string CampoCfg014 { get; set; }
        public string CampoCfg015 { get; set; }
        public string CampoCfg016 { get; set; }
        public string CampoCfg017 { get; set; }
        public string CampoCfg018 { get; set; }
        public string CampoCfg019 { get; set; }
        public string CampoCfg020 { get; set; }
        public string CampoCfgInt001 { get; set; }
        public string CampoCfgInt002 { get; set; }
        public string CampoCfgInt003 { get; set; }
        public string CampoCfgInt004 { get; set; }
        public string CampoCfgInt005 { get; set; }
        public string CampoCfgInt006 { get; set; }
        public string CampoCfgInt007 { get; set; }
        public string CampoCfgInt008 { get; set; }
        public string CampoCfgInt009 { get; set; }
        public string CampoCfgInt010 { get; set; }
        public string CampoCfgInt011 { get; set; }
        public string CampoCfgInt012 { get; set; }
        public string CampoCfgInt013 { get; set; }
        public string CampoCfgInt014 { get; set; }
        public string CampoCfgInt015 { get; set; }
        public string CampoCfgInt016 { get; set; }
        public string CampoCfgInt017 { get; set; }
        public string CampoCfgInt018 { get; set; }
        public string CampoCfgInt019 { get; set; }
        public string CampoCfgInt020 { get; set; }
        public string CampoCfgInt021 { get; set; }
        public string CampoCfgInt022 { get; set; }
        public string CampoCfgInt023 { get; set; }
        public string CampoCfgInt024 { get; set; }
        public string CampoCfgInt025 { get; set; }
        public string CampoCfgInt026 { get; set; }
        public string CampoCfgInt027 { get; set; }
        public string CampoCfgInt028 { get; set; }
        public string CampoCfgInt029 { get; set; }
        public string CampoCfgInt030 { get; set; }
        public string OrigemMercadoria { get; set; }
        public string ControladoIbama { get; set; }
        public string CodigoNcm { get; set; }
        public int CfopNaoContribuinte { get; set; }
        public int CfopContribuinte { get; set; }
        public double UnidadeTributavel { get; set; }
        public string ProdutoKitRegraFiscal { get; set; }
        public double CustoDoProduto { get; set; }
        public string CodigoExternoFornecedor { get; set; }
        public double DescontoMaxProduto { get; set; }
        public string PreVenda { get; set; }
        public string PreVendaDataInicio { get; set; }
        public string PreVendaDataFinal { get; set; }
        public string CodigoProdutoParceiro { get; set; }
        public string CodigoProdutoPaiParceiro { get; set; }
        public DescritorSimplesViewModel DescritorSimples { get; set; }
        public DescritorPreDefinidoViewModel DescritorPreDefinido { get; set; }
        public AtributosEstendidosViewModel AtributosEstendidos { get; set; }
        public CaracteristicasComplementaresViewModel CaracteristicasComplementares { get; set; }
        public ProdutosSubstitutosViewModel ProdutosSubstitutos { get; set; }
        public CategoriasDoSiteViewModel CategoriasDoSite { get; set; }
        public ProdutosPersonalizacaoViewModel ProdutosPersonalizacao { get; set; }
        public ComponentesKitViewModel ComponentesKit { get; set; }
        public ProdutosAssociadosViewModel ProdutosAssociados { get; set; }
        public string DescricaoClasse { get; set; }
        public string DescricaoMarca { get; set; }
        public string DescricaoFamilia { get; set; }
        public string DescricaoGrupo { get; set; }
        public string DescricaoSubgrupo { get; set; }
        public int CodigoCategoriaFiscal { get; set; }
        public int CodigoClassificacaoFiscal { get; set; }
        public string ClassificacaoFiscal { get; set; }
        public string HarmonizedCode { get; set; }
        public int DiasGarantia { get; set; }
        public bool PodeSerBrinde { get; set; }
        public int TipoRastreabilidade { get; set; }
        public string TipoControleEstoque { get; set; }
        public bool ProdutoKit { get; set; }
        public bool EfetuarCheckOut { get; set; }
        public bool DescricaoNotaFiscal { get; set; }
        public string ClassificacaoPersonalizacao { get; set; }
        public bool NaoImprimirEtqEntrada { get; set; }
        public string EstoqueLocalidade01 { get; set; }
        public string EstoqueLocalidade02 { get; set; }
        public string EstoqueLocalidade03 { get; set; }
        public string EstoqueLocalidade04 { get; set; }
        public string EstoqueLocalidade05 { get; set; }
        public string IdentificadorProduto { get; set; }
        public string IdentificadorProdutoPai { get; set; }
        public object ProdutosAtributosEstendidos { get; set; }
        public DadosLivrosViewModel DadosLivros { get; set; }
        public double PrecoTabela1 { get; set; }
        public double PrecoPromocao1 { get; set; }
        public string InicioPromocao1 { get; set; }
        public string TerminoPromocao1 { get; set; }
        public double PrecoTabela2 { get; set; }
        public double PrecoPromocao2 { get; set; }
        public string InicioPromocao2 { get; set; }
        public string TerminoPromocao2 { get; set; }
        public DadosImagemProdutoViewModel DadosImagemProduto { get; set; }
        public ClassificacoesComplementaresViewModel ClassificacoesComplementares { get; set; }
        public TransportadorasHabilitadasViewModel TransportadorasHabilitadas { get; set; }
        public int CodigoProdutoPaiAbacos { get; set; }
        public bool ReceitaRequerida { get; set; }
    }
}
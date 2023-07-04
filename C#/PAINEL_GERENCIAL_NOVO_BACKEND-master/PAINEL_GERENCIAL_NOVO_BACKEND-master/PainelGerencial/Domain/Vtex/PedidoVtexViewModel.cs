using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RecursosCompartilhados.ViewModels.Vtex
{
    [Serializable]
    public class PedidoVtexViewModel : ICloneable
    {
        [JsonProperty("orderId")]
        public string PedidoCod { get; set; }

        [JsonProperty("sequence")]
        public string Sequencia { get; set; }

        [JsonProperty("marketplaceOrderId")]
        public string PedidoMarketplaceCod { get; set; }

        [JsonProperty("marketplaceServicesEndpoint")]
        public string EndpointMarketplace { get; set; }

        [JsonProperty("sellerOrderId")]
        public string PedidoVendedor { get; set; }

        [JsonProperty("origin")]
        public string Origem { get; set; }

        [JsonProperty("affiliateId")]
        public string AfiliadoCod { get; set; }

        [JsonProperty("salesChannel")]
        public string CanalVenda { get; set; }

        [JsonProperty("merchantName")]
        public string Comerciante { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("statusDescription")]
        public string StatusDescricao { get; set; }

        [JsonProperty("value")]
        public long Valor { get; set; }

        [JsonProperty("creationDate")]
        public DateTime DataCriacao { get; set; }

        [JsonProperty("lastChange")]
        public DateTime DataUltimaAtualizacao { get; set; }

        [JsonProperty("orderGroup")]
        public string PedidoGrupo { get; set; }

        [JsonProperty("totals")]
        public List<TotaisPedidoVtexViewModel> Totais { get; set; }

        [JsonProperty("items")]
        public List<ItemPedidoVtexViewModel> Items { get; set; }

        //[JsonProperty("marketplaceItems")]
        //public List<> MarketplaceItems { get; set; }

        [JsonProperty("itemMetadata")]
        public ItemMetadata ItemMetadata { get; set; }

        [JsonProperty("clientProfileData")]
        public ClienteDadosPedidoVtexViewModel ClienteDados { get; set; }

        [JsonProperty("giftRegistryData")]
        public string RegistroPresente { get; set; }

        [JsonProperty("marketingData")]
        public MarketingDadosPedidoVtexViewModel DadosMarketing { get; set; }

        [JsonProperty("ratesAndBenefitsData")]
        public DescontoBaneficiosPedidoVtexViewModel DadosDescontosBeneficios { get; set; }

        [JsonProperty("shippingData")]
        public DadosEntregaPedidoVtexViewModel EntregaDados { get; set; }

        [JsonProperty("paymentData")]
        public PagamentoPedidoVtexViewModel PagamentoDados { get; set; }

        [JsonProperty("packageAttachment")]
        public PacoteAnexoPedidoVtexViewModel AnexoPacote { get; set; }

        [JsonProperty("sellers")]
        public List<VendedorPedidoVtexViewModel> Vendedores { get; set; }

        [JsonProperty("callCenterOperatorData")]
        public CallCenterPedidoVtexViewModel OperadorDados { get; set; }

        [JsonProperty("followUpEmail")]
        public string EmailFollowUp { get; set; }

        [JsonProperty("lastMessage")]
        public string UltimaMensagem { get; set; }

        [JsonProperty("hostname")]
        public string Hostname { get; set; }

        [JsonProperty("roundingError")]
        public int Erro { get; set; }

        [JsonProperty("orderFormId")]
        public string PedidoFormularioCod { get; set; }

        public bool NotificarPreAnalise { get; set; }
        public bool PartPier8 { get; set; }

        public static explicit operator PedidoVtexViewModel(Task<PedidoVtexViewModel> v)
        {
            throw new NotImplementedException();
        }

        #region ICloneable Members

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        #endregion
    }

    [Serializable]
    public class TotaisPedidoVtexViewModel
    {
        [JsonProperty("id")]
        public string Codigo { get; set; }

        [JsonProperty("name")]
        public string Nome { get; set; }

        [JsonProperty("value")]
        public long Valor { get; set; }
    }

    [Serializable]
    public class ItemPedidoVtexViewModel
    {
        [JsonProperty("uniqueId")]
        public string CodigoUnico { get; set; }

        [JsonProperty("id")]
        public string Codigo { get; set; }

        [JsonProperty("productId")]
        public string ProdutoCod { get; set; }

        [JsonProperty("ean")]
        public string CodigoBarras { get; set; }

        [JsonProperty("lockId")]
        public string CodigoFechado { get; set; }

        //[JsonProperty("itemAttachment")]
        //public List<> ItemAttachment { get; set; }

        //[JsonProperty("attachments")]
        //public List<> ItemAttachment { get; set; }

        [JsonProperty("quantity")]
        public int Quantidade { get; set; }

        [JsonProperty("seller")]
        public string Vendedor { get; set; }

        [JsonProperty("name")]
        public string Nome { get; set; }

        [JsonProperty("price")]
        public long Preco { get; set; }

        [JsonProperty("refId")]
        public string ReferenciaCod { get; set; }

        [JsonProperty("listPrice")]
        public long? PrecoListado { get; set; }

        //[JsonProperty("priceTags")]
        //public List<> PriceTags { get; set; }

        [JsonProperty("imageUrl")]
        public string ImagemUrl { get; set; }

        [JsonProperty("detailUrl")]
        public string DetalhesUrl { get; set; }

        [JsonProperty("components")]
        public List<ComponentesItemVtexViewModel> Componentes { get; set; }

        //[JsonProperty("bundleItems")]
        //public List<> BundleItems { get; set; }

        //[JsonProperty("params")]
        //public List<> Params { get; set; }

        //[JsonProperty("offerings")]
        //public List<> Offerings { get; set; }

        [JsonProperty("sellerSku")]
        public string VendedorSku { get; set; }

        [JsonProperty("priceValidUntil")]
        public string PrecoValidoAte { get; set; }

        [JsonProperty("commission")]
        public long? Comissao { get; set; }

        [JsonProperty("tax")]
        public long? Taxa { get; set; }

        [JsonProperty("preSaleDate")]
        public string DataPreVenda { get; set; }

        [JsonProperty("additionalInfo")]
        public InfoAdicionalItemVtexViewModel InformacoesAdicionais { get; set; }

        [JsonProperty("measurementUnit")]
        public string UnidadeMedida { get; set; }

        [JsonProperty("unitMultiplier")]
        public double UnidadeMultiplicador { get; set; }

        [JsonProperty("sellingPrice")]
        public long? PrecoVenda { get; set; }

        [JsonProperty("isGift")]
        public bool Presente { get; set; }

        [JsonProperty("shippingPrice")]
        public string PrecoEntrega { get; set; } // Tipo
    }

    [Serializable]
    public class ComponentesItemVtexViewModel
    {
        [JsonProperty("uniqueId")]
        public string CodigoUnico { get; set; }

        [JsonProperty("id")]
        public string Codigo { get; set; }

        [JsonProperty("productId")]
        public string ProdutoCod { get; set; }

        [JsonProperty("ean")]
        public string CodigoBarras { get; set; }

        [JsonProperty("lockId")]
        public string CodigoFechado { get; set; }

        //[JsonProperty("itemAttachment")]
        //public List<> ItemAttachment { get; set; }

        //[JsonProperty("attachments")]
        //public List<> ItemAttachment { get; set; }

        [JsonProperty("quantity")]
        public int Quantidade { get; set; }

        [JsonProperty("seller")]
        public string Vendedor { get; set; }

        [JsonProperty("name")]
        public string Nome { get; set; }

        [JsonProperty("price")]
        public long Preco { get; set; }

        [JsonProperty("refId")]
        public string ReferenciaCod { get; set; }

        [JsonProperty("listPrice")]
        public long? ListaPreco { get; set; }

        //[JsonProperty("priceTags")]
        //public List<> PriceTags { get; set; }

        [JsonProperty("imageUrl")]
        public string ImagemUrl { get; set; }

        [JsonProperty("detailUrl")]
        public string DetalhesUrl { get; set; }

        [JsonProperty("components")]
        public List<ComponentesItemVtexViewModel> Componentes { get; set; }

        //[JsonProperty("bundleItems")]
        //public List<> BundleItems { get; set; }

        //[JsonProperty("params")]
        //public List<> Params { get; set; }

        //[JsonProperty("offerings")]
        //public List<> Offerings { get; set; }

        [JsonProperty("sellerSku")]
        public string VendedorSku { get; set; }

        [JsonProperty("priceValidUntil")]
        public string PrecoValidoAte { get; set; }

        [JsonProperty("commission")]
        public double? Comissao { get; set; }

        [JsonProperty("tax")]
        public double? Taxa { get; set; }

        [JsonProperty("preSaleDate")]
        public string DataPreVenda { get; set; }

        [JsonProperty("additionalInfo")]
        public InfoAdicionalItemVtexViewModel InformacoesAdicionais { get; set; }

        [JsonProperty("measurementUnit")]
        public string UnidadeMedida { get; set; }

        [JsonProperty("unitMultiplier")]
        public double UnidadeMultiplicador { get; set; }

        [JsonProperty("sellingPrice")]
        public long? PrecoVenda { get; set; }

        [JsonProperty("isGift")]
        public bool Presente { get; set; }

        [JsonProperty("shippingPrice")]
        public string PrecoEntrega { get; set; } // Tipo

    }

    [Serializable]
    public class InfoAdicionalItemVtexViewModel
    {
        [JsonProperty("brandName")]
        public string MarcaNome { get; set; }

        [JsonProperty("brandId")]
        public string MarcaCod { get; set; }

        [JsonProperty("categoriesIds")]
        public string CategoriaCods { get; set; }

        //Text
        [JsonProperty("productClusterId")]
        public string ProdutoGrupoCod { get; set; }

        [JsonProperty("commercialConditionId")]
        public string CondicaoComercialCod { get; set; }

        [JsonProperty("dimension")]
        public DimensoesItemVtexViewModel Dimensao { get; set; }

        [JsonProperty("offeringInfo")]
        public string OfertaInformacao { get; set; }

        [JsonProperty("offeringType")]
        public string OfertaTipo { get; set; }

        [JsonProperty("offeringTypeId")]
        public string OfertaTipoCod { get; set; }
    }

    [Serializable]
    public class DimensoesItemVtexViewModel
    {
        [JsonProperty("cubicweight")]
        public double? PesoCubico { get; set; }

        [JsonProperty("height")]
        public double? Altura { get; set; }

        [JsonProperty("length")]
        public double? Comprimento { get; set; }

        [JsonProperty("weight")]
        public double? Peso { get; set; }

        [JsonProperty("width")]
        public double? Largura { get; set; }
    }

    [Serializable]
    public class ClienteDadosPedidoVtexViewModel
    {
        [JsonProperty("id")]
        public string Codigo { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("firstName")]
        public string PrimeiroNome { get; set; }

        [JsonProperty("lastName")]
        public string UltimoNome { get; set; }

        [JsonProperty("documentType")]
        public string DocumentoTipo { get; set; }

        [JsonProperty("document")]
        public string Documento { get; set; }

        [JsonProperty("phone")]
        public string Telefone { get; set; }

        [JsonProperty("corporateName")]
        public string EmpresaNome { get; set; }

        [JsonProperty("tradeName")]
        public string ComercioNome { get; set; }

        [JsonProperty("corporateDocument")]
        public string EmpresaDocumento { get; set; }

        [JsonProperty("stateInscription")]
        public string InscricaoEstadual { get; set; }

        [JsonProperty("corporatePhone")]
        public string EmpresaTelefone { get; set; }

        [JsonProperty("isCorporate")]
        public bool Empresa { get; set; }

        [JsonProperty("userProfileId")]
        public string PerfilCod { get; set; }
    }

    [Serializable]
    public class DescontoBaneficiosPedidoVtexViewModel
    {
        [JsonProperty("id")]
        public string Codigo { get; set; }

        [JsonProperty("rateAndBenefitsIdentifiers")]
        public List<DescontoBeneficiosIdentificadorPedidoViewModel> DescontoBeneficioIdentifcador { get; set; }
    }

    [Serializable]
    public class DescontoBeneficiosIdentificadorPedidoViewModel
    {
        [JsonProperty("description")]
        public string Descricao { get; set; }

        [JsonProperty("featured")]
        public bool Novidade { get; set; }

        [JsonProperty("id")]
        public string Codigo { get; set; }

        [JsonProperty("name")]
        public string Nome { get; set; }
    }

    [Serializable]
    public class DadosEntregaPedidoVtexViewModel
    {
        [JsonProperty("id")]
        public string Codigo { get; set; }

        [JsonProperty("address")]
        public EnderecoDadosPedidoVtexViewModel Endereco { get; set; }

        [JsonProperty("logisticsInfo")]
        public List<LogistaDadosPedidoVtexViewModel> LogistacaInformacoes { get; set; }
        
        [JsonProperty("trackingHints")]
        public List<DicasRastreamentoViewModel> DicasRastreamento { get; set; } 
    }

    [Serializable]
    public class EnderecoDadosPedidoVtexViewModel
    {
        [JsonProperty("addressType")]
        public string TipoEndereco { get; set; }

        [JsonProperty("receiverName")]
        public string Recebedor { get; set; }

        [JsonProperty("addressId")]
        public string EnderecoCod { get; set; }

        [JsonProperty("postalCode")]
        public string CodigoPostal { get; set; }

        [JsonProperty("city")]
        public string Cidade { get; set; }

        [JsonProperty("state")]
        public string Estado { get; set; }

        [JsonProperty("country")]
        public string Pais { get; set; }

        [JsonProperty("street")]
        public string Rua { get; set; }

        [JsonProperty("number")]
        public string Numero { get; set; }

        [JsonProperty("neighborhood")]
        public string Bairro { get; set; }

        [JsonProperty("complement")]
        public string Complemento { get; set; }

        [JsonProperty("reference")]
        public string Referencia { get; set; }
    }

    [Serializable]
    public class LogistaDadosPedidoVtexViewModel 
    {
        [JsonProperty("itemIndex")]
        public int ItemIndice { get; set; }

        [JsonProperty("selectedSla")]
        public string SlaSelecionado { get; set; }

        [JsonProperty("lockTTL")]
        public string TtlFechado { get; set; }

        [JsonProperty("price")]
        public long? Preco { get; set; }

        [JsonProperty("listPrice")]
        public long? PrecoLista { get; set; }

        [JsonProperty("sellingPrice")]
        public long? PrecoVendedor { get; set; }

        [JsonProperty("deliveryWindow")]
        public string JanelaEntrega { get; set; } //Tipo

        [JsonProperty("deliveryCompany")]
        public string EntregaCompanhia { get; set; }

        [JsonProperty("shippingEstimate")]
        public string PrazoEstimado { get; set; }

        [JsonProperty("shippingEstimateDate")]
        public string DataPrazoEstimado { get; set; }

        [JsonProperty("slas")]
        public List<LogistaSlasPedidoVtexViewModel> Slas { get; set; }

        [JsonProperty("shipsTo")]
        public List<string> Navios { get; set; }

        [JsonProperty("deliveryIds")]
        public List<LogistaEntregaPedidoVtexViewModel> EntregadorCod { get; set; }

    }


    [Serializable]
    public class DicasRastreamentoViewModel
    {
        [JsonProperty("courierName")]
        public string nomeEntregador { get; set; }

        [JsonProperty("trackingId")]
        public string IdRastreio { get; set; }

        [JsonProperty("trackingLabel")]
        public string EtiquetaRastreio { get; set; }

        [JsonProperty("trackingUrl")]
        public string URLRastreio { get; set; }
    }


    [Serializable]
    public class LogistaSlasPedidoVtexViewModel
    {
        [JsonProperty("id")]
        public string Codigo { get; set; }

        [JsonProperty("name")]
        public string Nome { get; set; }

        [JsonProperty("shippingEstimate")]
        public string PrazoEstimado { get; set; }

        [JsonProperty("deliveryWindow")]
        public string JanelaEntrega { get; set; }

        [JsonProperty("price")]
        public long? Preco { get; set; }
    }

    [Serializable]
    public class LogistaEntregaPedidoVtexViewModel
    {
        [JsonProperty("courierId")]
        public string TransportadorCod { get; set; }

        [JsonProperty("courierName")]
        public string TransportadorNome { get; set; }

        [JsonProperty("dockId")]
        public string DocaCod { get; set; }

        [JsonProperty("quantity")]
        public int Quantiade { get; set; } //Tipo

        [JsonProperty("warehouseId")]
        public string Armazemcod { get; set; }
    }

    [Serializable]
    public class PagamentoPedidoVtexViewModel
    {
        [JsonProperty("transactions")]
        public List<TransacoesPedidoVtexViewModel> Transacoes { get; set; }
    }

    [Serializable]
    public class TransacoesPedidoVtexViewModel
    {
        [JsonProperty("isActive")]
        public bool Ativa { get; set; }

        [JsonProperty("transactionId")]
        public string TransacaoCod { get; set; }

        [JsonProperty("payments")]
        public List<TransacaoPagamentoPedidoVtexViewModel> Pagamentos { get; set; }
    }

    [Serializable]
    public class TransacaoPagamentoPedidoVtexViewModel
    {
        [JsonProperty("id")]
        public string Codigo { get; set; }

        [JsonProperty("paymentSystem")]
        public string SistemaPagamento { get; set; }

        [JsonProperty("paymentSystemName")]
        public string PagamentoNome { get; set; }

        [JsonProperty("value")]
        public long Valor { get; set; }

        [JsonProperty("installments")]
        public int Parcelas { get; set; }

        [JsonProperty("referenceValue")]
        public long ReferenciaValor { get; set; }

        [JsonProperty("cardHolder")]
        public string CardHolder { get; set; } //Tipo

        [JsonProperty("cardNumber")]
        public string TitularCartao { get; set; } //Tipo

        [JsonProperty("firstDigits")]
        public string PrimeiroDigito { get; set; } //Tipo

        [JsonProperty("lastDigits")]
        public string UltimoDigito { get; set; } //Tipo

        [JsonProperty("cvv2")]
        public string Cvv2 { get; set; } //Tipo

        [JsonProperty("expireMonth")]
        public string MesExpiracao { get; set; } //Tipo

        [JsonProperty("expireYear")]
        public string AnoExpiracao { get; set; } //Tipo

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("giftCardId")]
        public string CartaoPresenteCod { get; set; } //Tipo

        [JsonProperty("giftCardName")]
        public string CartaoPresenteNome { get; set; }

        [JsonProperty("giftCardCaption")]
        public string CartaoPresenteTitulo { get; set; } //Tipo

        [JsonProperty("redemptionCode")]
        public string ResgateCod { get; set; }

        [JsonProperty("group")]
        public string Grupo { get; set; }

        [JsonProperty("tid")]
        public string Tid { get; set; } //Tipo

        [JsonProperty("dueDate")]
        public string DataVencimento { get; set; } //Tipo

        [JsonProperty("connectorResponses")]
        public ConectoresPedidoVtexViewModel ConnectorRespostas { get; set; }
    }

    [Serializable]
    public class VendedorPedidoVtexViewModel
    {
        [JsonProperty("id")]
        public string Codigo { get; set; }

        [JsonProperty("name")]
        public string Nome { get; set; }

        [JsonProperty("logo")]
        public string Logo { get; set; } //Tipo
    }

    [Serializable]
    public class ConectoresPedidoVtexViewModel
    {
        [JsonProperty("orderKey")]
        public string PedidoChave { get; set; }

        [JsonProperty("ReturnCode")]
        public string RetornoCod { get; set; }

        [JsonProperty("Message")]
        public string Mensagem { get; set; }

        [JsonProperty("authId")]
        public string AutenticacaoCod { get; set; }

        [JsonProperty("transactionIdentifier")]
        public string TransacaoIdentificador { get; set; }

        [JsonProperty("tid")]
        public string Tid { get; set; }

        [JsonProperty("nsu")]
        public string Nsu { get; set; }

        [JsonProperty("authorizationCode")]
        public string AutorizacaoCod { get; set; }

        [JsonProperty("acquirer")]
        public string Adquirente { get; set; }
    }

    [Serializable]
    public class MarketingDadosPedidoVtexViewModel
    {
        [JsonProperty("id")]
        public string Codigo { get; set; }

        [JsonProperty("utmSource")]
        public string UtmCodigo { get; set; }

        [JsonProperty("utmPartner")]
        public string UtmParceiro { get; set; }

        [JsonProperty("utmMedium")]
        public string UtmIntermediario { get; set; }

        [JsonProperty("utmCampaign")]
        public string UtmCampanha { get; set; }

        [JsonProperty("coupon")]
        public string Cupom { get; set; }

        [JsonProperty("utmiCampaign")]
        public string UtmiCampanha { get; set; }

        [JsonProperty("utmipage")]
        public string UtmiPagina { get; set; }

        [JsonProperty("utmiPart")]
        public string UtmiGrupo { get; set; }
    }

    [Serializable]
    public class CallCenterPedidoVtexViewModel
    {
        [JsonProperty("id")]
        public string Codigo { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("userName")]
        public string UsuarioNome { get; set; }
    }

    [Serializable]
    public class PacoteAnexoPedidoVtexViewModel
    {
        [JsonProperty("items")]
        public List<AnexoItemPedidoVtexViewModel> Items { get; set; }

        [JsonProperty("courier")]
        public string Transportadora { get; set; }

        [JsonProperty("invoiceNumber")]
        public string FaturaNumero { get; set; }

        [JsonProperty("invoiceValue")]
        public long FaturaValor { get; set; }

        [JsonProperty("invoiceUrl")]
        public string FaturaUrl { get; set; }

        [JsonProperty("issuanceDate")]
        public DateTime EmissaoData { get; set; }

        [JsonProperty("trackingNumber")]
        public string RastreioNumero { get; set; }

        [JsonProperty("invoiceKey")]
        public string FaturaChave { get; set; }

        [JsonProperty("trackingUrl")]
        public string RastreioUrl { get; set; }

        [JsonProperty("embeddedInvoice")]
        public string FaturaIncorporada { get; set; }

        [JsonProperty("type")]
        public string Tipo { get; set; }

        [JsonProperty("courierStatus")]
        public TransportadoraAnexoStatusPedidoVtexViewModel TransportadoraStatus { get; set; }
    }

    [Serializable]
    public class AnexoItemPedidoVtexViewModel
    {
        [JsonProperty("itemIndex")]
        public int ItemIndice { get; set; }

        [JsonProperty("quantity")]
        public int Quantidade { get; set; }

        [JsonProperty("price")]
        public long Preco { get; set; }

        [JsonProperty("description")]
        public string Descricao { get; set; }
    }

    [Serializable]
    public class TransportadoraAnexoStatusPedidoVtexViewModel
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("finished")]
        public bool Finalizado { get; set; }

        [JsonProperty("data")]
        public List<TransportadoraStatusDadosPedidoVtexViewModel> Dados { get; set; }
    }

    [Serializable]
    public class TransportadoraStatusDadosPedidoVtexViewModel
    {
        [JsonProperty("lastChange")]
        public string UltimaAtualizacao { get; set; }

        [JsonProperty("city")]
        public string Cidade { get; set; }

        [JsonProperty("state")]
        public string Estado { get; set; }

        [JsonProperty("description")]
        public string Descricao { get; set; }
    }

    public class VtexPaymentDetailsViewModel
    {
        public string status { get; set; }
        public bool isActive { get; set; }
        public string transactionId { get; set; }
        public string merchantName { get; set; }
        public List<Payment> payments { get; set; }
    }
    public class ConnectorResponses
    {
        public string Tid { get; set; }
        public object ReturnCode { get; set; }
        public object Message { get; set; }
        public string authId { get; set; }
    }
    public class SellersVtexViewModel
    {
        public int id { get; set; }
        public string name { get; set; }
    }
    public class Payment
    {
        public string id { get; set; }
        public string paymentSystem { get; set; }
        public string paymentSystemName { get; set; }
        public int value { get; set; }
        public int installments { get; set; }
        public int referenceValue { get; set; }
        public object cardHolder { get; set; }
        public object cardNumber { get; set; }
        public string firstDigits { get; set; }
        public string lastDigits { get; set; }
        public object cvv2 { get; set; }
        public object expireMonth { get; set; }
        public object expireYear { get; set; }
        public object url { get; set; }
        public object giftCardId { get; set; }
        public object giftCardName { get; set; }
        public object giftCardCaption { get; set; }
        public object redemptionCode { get; set; }
        public string group { get; set; }
        public string tid { get; set; }
        public object dueDate { get; set; }
        public ConnectorResponses connectorResponses { get; set; }
    }
    [Serializable]
    public partial class ItemMetadata
    {
        [JsonProperty("Items")]
        public SubItemVtexViewModel[] Items { get; set; }
    }
    [Serializable]
    public partial class SubItemVtexViewModel
    {
        [JsonProperty("Id")]
        public long Id { get; set; }

        [JsonProperty("Seller")]
        public string Seller { get; set; }

        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("SkuName")]
        public string SkuName { get; set; }

        [JsonProperty("ProductId")]
        public long ProductId { get; set; }

        [JsonProperty("RefId")]
        public string RefId { get; set; }

        [JsonProperty("Ean")]
        public string Ean { get; set; }

        [JsonProperty("ImageUrl")]
        public Uri ImageUrl { get; set; }

        [JsonProperty("DetailUrl")]
        public string DetailUrl { get; set; }
    }
}

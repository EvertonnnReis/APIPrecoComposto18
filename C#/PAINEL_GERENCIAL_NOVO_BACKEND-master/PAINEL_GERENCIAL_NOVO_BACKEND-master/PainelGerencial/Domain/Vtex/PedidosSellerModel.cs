using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using System.Text.Json.Serialization;

namespace PainelGerencial.Domain.Vtex
{
    public class PedidosSellerModel
    {
        [JsonProperty("orderId")]
        [JsonPropertyName("orderId")]
        public string OrderId { get; set; }

        [JsonProperty("sequence")]
        [JsonPropertyName("sequence")]
        public string Sequence { get; set; }

        [JsonProperty("marketplaceOrderId")]
        [JsonPropertyName("marketplaceOrderId")]
        public string MarketplaceOrderId { get; set; }

        [JsonProperty("marketplaceServicesEndpoint")]
        [JsonPropertyName("marketplaceServicesEndpoint")]
        public object MarketplaceServicesEndpoint { get; set; }

        [JsonProperty("sellerOrderId")]
        [JsonPropertyName("sellerOrderId")]
        public string SellerOrderId { get; set; }

        [JsonProperty("origin")]
        [JsonPropertyName("origin")]
        public string Origin { get; set; }

        [JsonProperty("affiliateId")]
        [JsonPropertyName("affiliateId")]
        public string AffiliateId { get; set; }

        [JsonProperty("salesChannel")]
        [JsonPropertyName("salesChannel")]
        public string SalesChannel { get; set; }

        [JsonProperty("merchantName")]
        [JsonPropertyName("merchantName")]
        public object MerchantName { get; set; }

        [JsonProperty("status")]
        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonProperty("workflowIsInError")]
        [JsonPropertyName("workflowIsInError")]
        public bool WorkflowIsInError { get; set; }

        [JsonProperty("statusDescription")]
        [JsonPropertyName("statusDescription")]
        public string StatusDescription { get; set; }

        [JsonProperty("value")]
        [JsonPropertyName("value")]
        public int Value { get; set; }

        [JsonProperty("creationDate")]
        [JsonPropertyName("creationDate")]
        public DateTime CreationDate { get; set; }

        [JsonProperty("lastChange")]
        [JsonPropertyName("lastChange")]
        public DateTime LastChange { get; set; }

        [JsonProperty("orderGroup")]
        [JsonPropertyName("orderGroup")]
        public string OrderGroup { get; set; }

        [JsonProperty("totals")]
        [JsonPropertyName("totals")]
        public List<Total> Totals { get; set; }

        [JsonProperty("items")]
        [JsonPropertyName("items")]
        public List<Item> Items { get; set; }

        [JsonProperty("marketplaceItems")]
        [JsonPropertyName("marketplaceItems")]
        public List<object> MarketplaceItems { get; set; }

        [JsonProperty("clientProfileData")]
        [JsonPropertyName("clientProfileData")]
        public ClientProfileData ClientProfileData { get; set; }

        [JsonProperty("giftRegistryData")]
        [JsonPropertyName("giftRegistryData")]
        public object GiftRegistryData { get; set; }

        [JsonProperty("marketingData")]
        [JsonPropertyName("marketingData")]
        public MarketingData MarketingData { get; set; }

        [JsonProperty("ratesAndBenefitsData")]
        [JsonPropertyName("ratesAndBenefitsData")]
        public RatesAndBenefitsData RatesAndBenefitsData { get; set; }

        [JsonProperty("shippingData")]
        [JsonPropertyName("shippingData")]
        public ShippingData ShippingData { get; set; }

        [JsonProperty("paymentData")]
        [JsonPropertyName("paymentData")]
        public PaymentData PaymentData { get; set; }

        [JsonProperty("packageAttachment")]
        [JsonPropertyName("packageAttachment")]
        public PackageAttachment PackageAttachment { get; set; }

        [JsonProperty("sellers")]
        [JsonPropertyName("sellers")]
        public List<Seller> Sellers { get; set; }

        [JsonProperty("callCenterOperatorData")]
        [JsonPropertyName("callCenterOperatorData")]
        public object CallCenterOperatorData { get; set; }

        [JsonProperty("followUpEmail")]
        [JsonPropertyName("followUpEmail")]
        public string FollowUpEmail { get; set; }

        [JsonProperty("lastMessage")]
        [JsonPropertyName("lastMessage")]
        public object LastMessage { get; set; }

        [JsonProperty("hostname")]
        [JsonPropertyName("hostname")]
        public string Hostname { get; set; }

        [JsonProperty("invoiceDate")]
        [JsonPropertyName("invoiceDate")]
        public InvoiceDate? InvoiceDate { get; set; }

        [JsonProperty("changesAttachment")]
        [JsonPropertyName("changesAttachment")]
        public object ChangesAttachment { get; set; }

        [JsonProperty("openTextField")]
        [JsonPropertyName("openTextField")]
        public object OpenTextField { get; set; }

        [JsonProperty("roundingError")]
        [JsonPropertyName("roundingError")]
        public int RoundingError { get; set; }

        [JsonProperty("orderFormId")]
        [JsonPropertyName("orderFormId")]
        public string OrderFormId { get; set; }

        [JsonProperty("commercialConditionData")]
        [JsonPropertyName("commercialConditionData")]
        public object CommercialConditionData { get; set; }

        [JsonProperty("isCompleted")]
        [JsonPropertyName("isCompleted")]
        public bool IsCompleted { get; set; }

        [JsonProperty("customData")]
        [JsonPropertyName("customData")]
        public object CustomData { get; set; }

        [JsonProperty("storePreferencesData")]
        [JsonPropertyName("storePreferencesData")]
        public StorePreferencesData StorePreferencesData { get; set; }

        [JsonProperty("allowCancellation")]
        [JsonPropertyName("allowCancellation")]
        public bool AllowCancellation { get; set; }

        [JsonProperty("allowEdition")]
        [JsonPropertyName("allowEdition")]
        public bool AllowEdition { get; set; }

        [JsonProperty("isCheckedIn")]
        [JsonPropertyName("isCheckedIn")]
        public bool IsCheckedIn { get; set; }

        [JsonProperty("marketplace")]
        [JsonPropertyName("marketplace")]
        public object Marketplace { get; set; }

        [JsonProperty("authorizedDate")]
        [JsonPropertyName("authorizedDate")]
        public DateTime? AuthorizedDate { get; set; }

        [JsonProperty("invoicedDate")]
        [JsonPropertyName("invoicedDate")]
        public DateTime? InvoicedDate { get; set; }

        [JsonProperty("cancelReason")]
        [JsonPropertyName("cancelReason")]
        public object CancelReason { get; set; }

        [JsonProperty("itemMetadata")]
        [JsonPropertyName("itemMetadata")]
        public ItemMetadata ItemMetadata { get; set; }

        [JsonProperty("subscriptionData")]
        [JsonPropertyName("subscriptionData")]
        public object SubscriptionData { get; set; }

        [JsonProperty("taxData")]
        [JsonPropertyName("taxData")]
        public object TaxData { get; set; }

        [JsonProperty("checkedInPickupPointId")]
        [JsonPropertyName("checkedInPickupPointId")]
        public object CheckedInPickupPointId { get; set; }

        [JsonProperty("cancellationData")]
        [JsonPropertyName("cancellationData")]
        public object CancellationData { get; set; }

        [JsonProperty("clientPreferencesData")]
        [JsonPropertyName("clientPreferencesData")]
        public ClientPreferencesData ClientPreferencesData { get; set; }
    }

    public class AdditionalInfo
    {
        [JsonProperty("brandName")]
        [JsonPropertyName("brandName")]
        public string BrandName { get; set; }

        [JsonProperty("brandId")]
        [JsonPropertyName("brandId")]
        public string BrandId { get; set; }

        [JsonProperty("categoriesIds")]
        [JsonPropertyName("categoriesIds")]
        public string CategoriesIds { get; set; }

        [JsonProperty("categories")]
        [JsonPropertyName("categories")]
        public List<Category> Categories { get; set; }

        [JsonProperty("productClusterId")]
        [JsonPropertyName("productClusterId")]
        public string ProductClusterId { get; set; }

        [JsonProperty("commercialConditionId")]
        [JsonPropertyName("commercialConditionId")]
        public string CommercialConditionId { get; set; }

        [JsonProperty("dimension")]
        [JsonPropertyName("dimension")]
        public Dimension Dimension { get; set; }

        [JsonProperty("offeringInfo")]
        [JsonPropertyName("offeringInfo")]
        public object OfferingInfo { get; set; }

        [JsonProperty("offeringType")]
        [JsonPropertyName("offeringType")]
        public object OfferingType { get; set; }

        [JsonProperty("offeringTypeId")]
        [JsonPropertyName("offeringTypeId")]
        public object OfferingTypeId { get; set; }
    }

    public class Address
    {
        [JsonProperty("addressType")]
        [JsonPropertyName("addressType")]
        public string AddressType { get; set; }

        [JsonProperty("receiverName")]
        [JsonPropertyName("receiverName")]
        public string ReceiverName { get; set; }

        [JsonProperty("addressId")]
        [JsonPropertyName("addressId")]
        public string AddressId { get; set; }

        [JsonProperty("versionId")]
        [JsonPropertyName("versionId")]
        public object VersionId { get; set; }

        [JsonProperty("entityId")]
        [JsonPropertyName("entityId")]
        public object EntityId { get; set; }

        [JsonProperty("postalCode")]
        [JsonPropertyName("postalCode")]
        public string PostalCode { get; set; }

        [JsonProperty("city")]
        [JsonPropertyName("city")]
        public string City { get; set; }

        [JsonProperty("state")]
        [JsonPropertyName("state")]
        public string State { get; set; }

        [JsonProperty("country")]
        [JsonPropertyName("country")]
        public string Country { get; set; }

        [JsonProperty("street")]
        [JsonPropertyName("street")]
        public string Street { get; set; }

        [JsonProperty("number")]
        [JsonPropertyName("number")]
        public string Number { get; set; }

        [JsonProperty("neighborhood")]
        [JsonPropertyName("neighborhood")]
        public string Neighborhood { get; set; }

        [JsonProperty("complement")]
        [JsonPropertyName("complement")]
        public string Complement { get; set; }

        [JsonProperty("reference")]
        [JsonPropertyName("reference")]
        public object Reference { get; set; }

        [JsonProperty("geoCoordinates")]
        [JsonPropertyName("geoCoordinates")]
        public List<double> GeoCoordinates { get; set; }
    }

    public class BillingAddress
    {
        [JsonProperty("postalCode")]
        [JsonPropertyName("postalCode")]
        public string PostalCode { get; set; }

        [JsonProperty("city")]
        [JsonPropertyName("city")]
        public string City { get; set; }

        [JsonProperty("state")]
        [JsonPropertyName("state")]
        public string State { get; set; }

        [JsonProperty("country")]
        [JsonPropertyName("country")]
        public string Country { get; set; }

        [JsonProperty("street")]
        [JsonPropertyName("street")]
        public string Street { get; set; }

        [JsonProperty("number")]
        [JsonPropertyName("number")]
        public string Number { get; set; }

        [JsonProperty("neighborhood")]
        [JsonPropertyName("neighborhood")]
        public string Neighborhood { get; set; }

        [JsonProperty("complement")]
        [JsonPropertyName("complement")]
        public string Complement { get; set; }

        [JsonProperty("reference")]
        [JsonPropertyName("reference")]
        public object Reference { get; set; }

        [JsonProperty("geoCoordinates")]
        [JsonPropertyName("geoCoordinates")]
        public List<double> GeoCoordinates { get; set; }
    }

    public class Category
    {
        [JsonProperty("id")]
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        [JsonPropertyName("name")]
        public string Name { get; set; }
    }

    public class ClientPreferencesData
    {
        [JsonProperty("locale")]
        [JsonPropertyName("locale")]
        public string Locale { get; set; }

        [JsonProperty("optinNewsLetter")]
        [JsonPropertyName("optinNewsLetter")]
        public bool OptinNewsLetter { get; set; }
    }

    public class ClientProfileData
    {
        [JsonProperty("id")]
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonProperty("email")]
        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonProperty("firstName")]
        [JsonPropertyName("firstName")]
        public string FirstName { get; set; }

        [JsonProperty("lastName")]
        [JsonPropertyName("lastName")]
        public string LastName { get; set; }

        [JsonProperty("documentType")]
        [JsonPropertyName("documentType")]
        public string DocumentType { get; set; }

        [JsonProperty("document")]
        [JsonPropertyName("document")]
        public string Document { get; set; }

        [JsonProperty("phone")]
        [JsonPropertyName("phone")]
        public string Phone { get; set; }

        [JsonProperty("corporateName")]
        [JsonPropertyName("corporateName")]
        public object CorporateName { get; set; }

        [JsonProperty("tradeName")]
        [JsonPropertyName("tradeName")]
        public object TradeName { get; set; }

        [JsonProperty("corporateDocument")]
        [JsonPropertyName("corporateDocument")]
        public object CorporateDocument { get; set; }

        [JsonProperty("stateInscription")]
        [JsonPropertyName("stateInscription")]
        public object StateInscription { get; set; }

        [JsonProperty("corporatePhone")]
        [JsonPropertyName("corporatePhone")]
        public object CorporatePhone { get; set; }

        [JsonProperty("isCorporate")]
        [JsonPropertyName("isCorporate")]
        public bool IsCorporate { get; set; }

        [JsonProperty("userProfileId")]
        [JsonPropertyName("userProfileId")]
        public string UserProfileId { get; set; }

        [JsonProperty("userProfileVersion")]
        [JsonPropertyName("userProfileVersion")]
        public object UserProfileVersion { get; set; }

        [JsonProperty("customerClass")]
        [JsonPropertyName("customerClass")]
        public object CustomerClass { get; set; }
    }

    public class ConnectorResponses
    {
        [JsonProperty("Tid")]
        [JsonPropertyName("Tid")]
        public string Tid { get; set; }

        [JsonProperty("ReturnCode")]
        [JsonPropertyName("ReturnCode")]
        public object ReturnCode { get; set; }

        [JsonProperty("Message")]
        [JsonPropertyName("Message")]
        public object Message { get; set; }

        [JsonProperty("authId")]
        [JsonPropertyName("authId")]
        public string AuthId { get; set; }

        [JsonProperty("nsu")]
        [JsonPropertyName("nsu")]
        public string Nsu { get; set; }

        [JsonProperty("acquirer")]
        [JsonPropertyName("acquirer")]
        public string Acquirer { get; set; }
    }

    public class Content
    {
    }

    public class CourierStatus
    {
        [JsonProperty("status")]
        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonProperty("finished")]
        [JsonPropertyName("finished")]
        public bool Finished { get; set; }

        [JsonProperty("deliveredDate")]
        [JsonPropertyName("deliveredDate")]
        public DateTime DeliveredDate { get; set; }

        [JsonProperty("data")]
        [JsonPropertyName("data")]
        public List<Datum> Data { get; set; }
    }

    public class CurrencyFormatInfo
    {
        [JsonProperty("CurrencyDecimalDigits")]
        [JsonPropertyName("CurrencyDecimalDigits")]
        public int CurrencyDecimalDigits { get; set; }

        [JsonProperty("CurrencyDecimalSeparator")]
        [JsonPropertyName("CurrencyDecimalSeparator")]
        public string CurrencyDecimalSeparator { get; set; }

        [JsonProperty("CurrencyGroupSeparator")]
        [JsonPropertyName("CurrencyGroupSeparator")]
        public string CurrencyGroupSeparator { get; set; }

        [JsonProperty("CurrencyGroupSize")]
        [JsonPropertyName("CurrencyGroupSize")]
        public int CurrencyGroupSize { get; set; }

        [JsonProperty("StartsWithCurrencySymbol")]
        [JsonPropertyName("StartsWithCurrencySymbol")]
        public bool StartsWithCurrencySymbol { get; set; }
    }

    public class Datum
    {
        [JsonProperty("lastChange")]
        [JsonPropertyName("lastChange")]
        public DateTime LastChange { get; set; }

        [JsonProperty("city")]
        [JsonPropertyName("city")]
        public string City { get; set; }

        [JsonProperty("state")]
        [JsonPropertyName("state")]
        public string State { get; set; }

        [JsonProperty("description")]
        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonProperty("createDate")]
        [JsonPropertyName("createDate")]
        public DateTime CreateDate { get; set; }
    }

    public class DeliveryChannel
    {
        [JsonProperty("id")]
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonProperty("stockBalance")]
        [JsonPropertyName("stockBalance")]
        public int StockBalance { get; set; }
    }

    public class Dimension
    {
        [JsonProperty("cubicweight")]
        [JsonPropertyName("cubicweight")]
        public double Cubicweight { get; set; }

        [JsonProperty("height")]
        [JsonPropertyName("height")]
        public double Height { get; set; }

        [JsonProperty("length")]
        [JsonPropertyName("length")]
        public double Length { get; set; }

        [JsonProperty("weight")]
        [JsonPropertyName("weight")]
        public double Weight { get; set; }

        [JsonProperty("width")]
        [JsonPropertyName("width")]
        public double Width { get; set; }
    }

    public class InvoiceDate
    {
        [JsonProperty("address")]
        [JsonPropertyName("address")]
        public object Address { get; set; }

        [JsonProperty("userPaymentInfo")]
        [JsonPropertyName("userPaymentInfo")]
        public object UserPaymentInfo { get; set; }
    }

    public class Item
    {
        [JsonProperty("uniqueId")]
        [JsonPropertyName("uniqueId")]
        public string UniqueId { get; set; }

        [JsonProperty("id")]
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonProperty("productId")]
        [JsonPropertyName("productId")]
        public string ProductId { get; set; }

        [JsonProperty("ean")]
        [JsonPropertyName("ean")]
        public string Ean { get; set; }

        [JsonProperty("lockId")]
        [JsonPropertyName("lockId")]
        public object LockId { get; set; }

        [JsonProperty("itemAttachment")]
        [JsonPropertyName("itemAttachment")]
        public ItemAttachment ItemAttachment { get; set; }

        [JsonProperty("attachments")]
        [JsonPropertyName("attachments")]
        public List<object> Attachments { get; set; }

        [JsonProperty("quantity")]
        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonProperty("seller")]
        [JsonPropertyName("seller")]
        public string Seller { get; set; }

        [JsonProperty("name")]
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonProperty("refId")]
        [JsonPropertyName("refId")]
        public string RefId { get; set; }

        [JsonProperty("price")]
        [JsonPropertyName("price")]
        public int Price { get; set; }

        [JsonProperty("listPrice")]
        [JsonPropertyName("listPrice")]
        public int ListPrice { get; set; }

        [JsonProperty("manualPrice")]
        [JsonPropertyName("manualPrice")]
        public object ManualPrice { get; set; }

        [JsonProperty("priceTags")]
        [JsonPropertyName("priceTags")]
        public List<object> PriceTags { get; set; }

        [JsonProperty("imageUrl")]
        [JsonPropertyName("imageUrl")]
        public string ImageUrl { get; set; }

        [JsonProperty("detailUrl")]
        [JsonPropertyName("detailUrl")]
        public string DetailUrl { get; set; }

        [JsonProperty("components")]
        [JsonPropertyName("components")]
        public List<object> Components { get; set; }

        [JsonProperty("bundleItems")]
        [JsonPropertyName("bundleItems")]
        public List<object> BundleItems { get; set; }

        [JsonProperty("params")]
        [JsonPropertyName("params")]
        public List<object> Params { get; set; }

        [JsonProperty("offerings")]
        [JsonPropertyName("offerings")]
        public List<object> Offerings { get; set; }

        [JsonProperty("attachmentOfferings")]
        [JsonPropertyName("attachmentOfferings")]
        public List<object> AttachmentOfferings { get; set; }

        [JsonProperty("sellerSku")]
        [JsonPropertyName("sellerSku")]
        public string SellerSku { get; set; }

        [JsonProperty("priceValidUntil")]
        [JsonPropertyName("priceValidUntil")]
        public object PriceValidUntil { get; set; }

        [JsonProperty("commission")]
        [JsonPropertyName("commission")]
        public int Commission { get; set; }

        [JsonProperty("tax")]
        [JsonPropertyName("tax")]
        public int Tax { get; set; }

        [JsonProperty("preSaleDate")]
        [JsonPropertyName("preSaleDate")]
        public object PreSaleDate { get; set; }

        [JsonProperty("additionalInfo")]
        [JsonPropertyName("additionalInfo")]
        public AdditionalInfo AdditionalInfo { get; set; }

        [JsonProperty("measurementUnit")]
        [JsonPropertyName("measurementUnit")]
        public string MeasurementUnit { get; set; }

        [JsonProperty("unitMultiplier")]
        [JsonPropertyName("unitMultiplier")]
        public double UnitMultiplier { get; set; }

        [JsonProperty("sellingPrice")]
        [JsonPropertyName("sellingPrice")]
        public int SellingPrice { get; set; }

        [JsonProperty("isGift")]
        [JsonPropertyName("isGift")]
        public bool IsGift { get; set; }

        [JsonProperty("shippingPrice")]
        [JsonPropertyName("shippingPrice")]
        public object ShippingPrice { get; set; }

        [JsonProperty("rewardValue")]
        [JsonPropertyName("rewardValue")]
        public int RewardValue { get; set; }

        [JsonProperty("freightCommission")]
        [JsonPropertyName("freightCommission")]
        public int FreightCommission { get; set; }

        [JsonProperty("priceDefinition")]
        [JsonPropertyName("priceDefinition")]
        public PriceDefinition PriceDefinition { get; set; }

        [JsonProperty("taxCode")]
        [JsonPropertyName("taxCode")]
        public string TaxCode { get; set; }

        [JsonProperty("parentItemIndex")]
        [JsonPropertyName("parentItemIndex")]
        public object ParentItemIndex { get; set; }

        [JsonProperty("parentAssemblyBinding")]
        [JsonPropertyName("parentAssemblyBinding")]
        public object ParentAssemblyBinding { get; set; }

        [JsonProperty("callCenterOperator")]
        [JsonPropertyName("callCenterOperator")]
        public object CallCenterOperator { get; set; }

        [JsonProperty("serialNumbers")]
        [JsonPropertyName("serialNumbers")]
        public object SerialNumbers { get; set; }

        [JsonProperty("assemblies")]
        [JsonPropertyName("assemblies")]
        public List<object> Assemblies { get; set; }

        [JsonProperty("costPrice")]
        [JsonPropertyName("costPrice")]
        public object CostPrice { get; set; }

        [JsonProperty("itemIndex")]
        [JsonPropertyName("itemIndex")]
        public int ItemIndex { get; set; }

        [JsonProperty("description")]
        [JsonPropertyName("description")]
        public object Description { get; set; }
    }

    public class Item3
    {
        [JsonProperty("Id")]
        [JsonPropertyName("Id")]
        public string Id { get; set; }

        [JsonProperty("Seller")]
        [JsonPropertyName("Seller")]
        public string Seller { get; set; }

        [JsonProperty("Name")]
        [JsonPropertyName("Name")]
        public string Name { get; set; }

        [JsonProperty("SkuName")]
        [JsonPropertyName("SkuName")]
        public string SkuName { get; set; }

        [JsonProperty("ProductId")]
        [JsonPropertyName("ProductId")]
        public string ProductId { get; set; }

        [JsonProperty("RefId")]
        [JsonPropertyName("RefId")]
        public string RefId { get; set; }

        [JsonProperty("Ean")]
        [JsonPropertyName("Ean")]
        public string Ean { get; set; }

        [JsonProperty("ImageUrl")]
        [JsonPropertyName("ImageUrl")]
        public string ImageUrl { get; set; }

        [JsonProperty("DetailUrl")]
        [JsonPropertyName("DetailUrl")]
        public string DetailUrl { get; set; }

        [JsonProperty("AssemblyOptions")]
        [JsonPropertyName("AssemblyOptions")]
        public List<object> AssemblyOptions { get; set; }
    }

    public class ItemAttachment
    {
        [JsonProperty("content")]
        [JsonPropertyName("content")]
        public Content Content { get; set; }

        [JsonProperty("name")]
        [JsonPropertyName("name")]
        public object Name { get; set; }
    }

    public class ItemMetadata
    {
        [JsonProperty("Items")]
        [JsonPropertyName("Items")]
        public List<Item> Items { get; set; }
    }

    public class LogisticsInfo
    {
        [JsonProperty("itemIndex")]
        [JsonPropertyName("itemIndex")]
        public int ItemIndex { get; set; }

        [JsonProperty("selectedSla")]
        [JsonPropertyName("selectedSla")]
        public string SelectedSla { get; set; }

        [JsonProperty("lockTTL")]
        [JsonPropertyName("lockTTL")]
        public string LockTTL { get; set; }

        [JsonProperty("price")]
        [JsonPropertyName("price")]
        public int Price { get; set; }

        [JsonProperty("listPrice")]
        [JsonPropertyName("listPrice")]
        public int ListPrice { get; set; }

        [JsonProperty("sellingPrice")]
        [JsonPropertyName("sellingPrice")]
        public int SellingPrice { get; set; }

        [JsonProperty("deliveryWindow")]
        [JsonPropertyName("deliveryWindow")]
        public object DeliveryWindow { get; set; }

        [JsonProperty("deliveryCompany")]
        [JsonPropertyName("deliveryCompany")]
        public object DeliveryCompany { get; set; }

        [JsonProperty("shippingEstimate")]
        [JsonPropertyName("shippingEstimate")]
        public string ShippingEstimate { get; set; }

        [JsonProperty("shippingEstimateDate")]
        [JsonPropertyName("shippingEstimateDate")]
        public DateTime? ShippingEstimateDate { get; set; }

        [JsonProperty("slas")]
        [JsonPropertyName("slas")]
        public List<Sla> Slas { get; set; }

        [JsonProperty("shipsTo")]
        [JsonPropertyName("shipsTo")]
        public List<string> ShipsTo { get; set; }

        [JsonProperty("deliveryIds")]
        [JsonPropertyName("deliveryIds")]
        public List<object> DeliveryIds { get; set; }

        [JsonProperty("deliveryChannels")]
        [JsonPropertyName("deliveryChannels")]
        public List<DeliveryChannel> DeliveryChannels { get; set; }

        [JsonProperty("deliveryChannel")]
        [JsonPropertyName("deliveryChannel")]
        public string DeliveryChannel { get; set; }

        [JsonProperty("pickupStoreInfo")]
        [JsonPropertyName("pickupStoreInfo")]
        public PickupStoreInfo PickupStoreInfo { get; set; }

        [JsonProperty("addressId")]
        [JsonPropertyName("addressId")]
        public string AddressId { get; set; }

        [JsonProperty("versionId")]
        [JsonPropertyName("versionId")]
        public object VersionId { get; set; }

        [JsonProperty("entityId")]
        [JsonPropertyName("entityId")]
        public object EntityId { get; set; }

        [JsonProperty("polygonName")]
        [JsonPropertyName("polygonName")]
        public object PolygonName { get; set; }

        [JsonProperty("pickupPointId")]
        [JsonPropertyName("pickupPointId")]
        public object PickupPointId { get; set; }

        [JsonProperty("transitTime")]
        [JsonPropertyName("transitTime")]
        public object TransitTime { get; set; }
    }

    public class MarketingData
    {
        [JsonProperty("id")]
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonProperty("utmSource")]
        [JsonPropertyName("utmSource")]
        public string UtmSource { get; set; }

        [JsonProperty("utmPartner")]
        [JsonPropertyName("utmPartner")]
        public object UtmPartner { get; set; }

        [JsonProperty("utmMedium")]
        [JsonPropertyName("utmMedium")]
        public string UtmMedium { get; set; }

        [JsonProperty("utmCampaign")]
        [JsonPropertyName("utmCampaign")]
        public string UtmCampaign { get; set; }

        [JsonProperty("coupon")]
        [JsonPropertyName("coupon")]
        public object Coupon { get; set; }

        [JsonProperty("utmiCampaign")]
        [JsonPropertyName("utmiCampaign")]
        public string UtmiCampaign { get; set; }

        [JsonProperty("utmipage")]
        [JsonPropertyName("utmipage")]
        public string Utmipage { get; set; }

        [JsonProperty("utmiPart")]
        [JsonPropertyName("utmiPart")]
        public string UtmiPart { get; set; }

        [JsonProperty("marketingTags")]
        [JsonPropertyName("marketingTags")]
        public List<object> MarketingTags { get; set; }
    }

    public class Package
    {
        [JsonProperty("items")]
        [JsonPropertyName("items")]
        public List<Item> Items { get; set; }

        [JsonProperty("courier")]
        [JsonPropertyName("courier")]
        public string Courier { get; set; }

        [JsonProperty("invoiceNumber")]
        [JsonPropertyName("invoiceNumber")]
        public string InvoiceNumber { get; set; }

        [JsonProperty("invoiceValue")]
        [JsonPropertyName("invoiceValue")]
        public int InvoiceValue { get; set; }

        [JsonProperty("invoiceUrl")]
        [JsonPropertyName("invoiceUrl")]
        public string InvoiceUrl { get; set; }

        [JsonProperty("issuanceDate")]
        [JsonPropertyName("issuanceDate")]
        public DateTime IssuanceDate { get; set; }

        [JsonProperty("trackingNumber")]
        [JsonPropertyName("trackingNumber")]
        public string TrackingNumber { get; set; }

        [JsonProperty("invoiceKey")]
        [JsonPropertyName("invoiceKey")]
        public string InvoiceKey { get; set; }

        [JsonProperty("trackingUrl")]
        [JsonPropertyName("trackingUrl")]
        public string TrackingUrl { get; set; }

        [JsonProperty("embeddedInvoice")]
        [JsonPropertyName("embeddedInvoice")]
        public string EmbeddedInvoice { get; set; }

        [JsonProperty("type")]
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonProperty("courierStatus")]
        [JsonPropertyName("courierStatus")]
        public CourierStatus CourierStatus { get; set; }

        [JsonProperty("cfop")]
        [JsonPropertyName("cfop")]
        public object Cfop { get; set; }

        [JsonProperty("restitutions")]
        [JsonPropertyName("restitutions")]
        public Restitutions Restitutions { get; set; }

        [JsonProperty("volumes")]
        [JsonPropertyName("volumes")]
        public object Volumes { get; set; }

        [JsonProperty("EnableInferItems")]
        [JsonPropertyName("EnableInferItems")]
        public object EnableInferItems { get; set; }
    }

    public class PackageAttachment
    {
        [JsonProperty("packages")]
        [JsonPropertyName("packages")]
        public List<Package> Packages { get; set; }
    }

    public class Payment
    {
        [JsonProperty("id")]
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonProperty("paymentSystem")]
        [JsonPropertyName("paymentSystem")]
        public string PaymentSystem { get; set; }

        [JsonProperty("paymentSystemName")]
        [JsonPropertyName("paymentSystemName")]
        public string PaymentSystemName { get; set; }

        [JsonProperty("value")]
        [JsonPropertyName("value")]
        public int Value { get; set; }

        [JsonProperty("installments")]
        [JsonPropertyName("installments")]
        public int Installments { get; set; }

        [JsonProperty("referenceValue")]
        [JsonPropertyName("referenceValue")]
        public int ReferenceValue { get; set; }

        [JsonProperty("cardHolder")]
        [JsonPropertyName("cardHolder")]
        public object CardHolder { get; set; }

        [JsonProperty("cardNumber")]
        [JsonPropertyName("cardNumber")]
        public object CardNumber { get; set; }

        [JsonProperty("firstDigits")]
        [JsonPropertyName("firstDigits")]
        public string FirstDigits { get; set; }

        [JsonProperty("lastDigits")]
        [JsonPropertyName("lastDigits")]
        public string LastDigits { get; set; }

        [JsonProperty("cvv2")]
        [JsonPropertyName("cvv2")]
        public object Cvv2 { get; set; }

        [JsonProperty("expireMonth")]
        [JsonPropertyName("expireMonth")]
        public object ExpireMonth { get; set; }

        [JsonProperty("expireYear")]
        [JsonPropertyName("expireYear")]
        public object ExpireYear { get; set; }

        [JsonProperty("url")]
        [JsonPropertyName("url")]
        public object Url { get; set; }

        [JsonProperty("giftCardId")]
        [JsonPropertyName("giftCardId")]
        public object GiftCardId { get; set; }

        [JsonProperty("giftCardName")]
        [JsonPropertyName("giftCardName")]
        public object GiftCardName { get; set; }

        [JsonProperty("giftCardCaption")]
        [JsonPropertyName("giftCardCaption")]
        public object GiftCardCaption { get; set; }

        [JsonProperty("redemptionCode")]
        [JsonPropertyName("redemptionCode")]
        public object RedemptionCode { get; set; }

        [JsonProperty("group")]
        [JsonPropertyName("group")]
        public string Group { get; set; }

        [JsonProperty("tid")]
        [JsonPropertyName("tid")]
        public string Tid { get; set; }

        [JsonProperty("dueDate")]
        [JsonPropertyName("dueDate")]
        public object DueDate { get; set; }

        [JsonProperty("connectorResponses")]
        [JsonPropertyName("connectorResponses")]
        public ConnectorResponses ConnectorResponses { get; set; }

        [JsonProperty("giftCardProvider")]
        [JsonPropertyName("giftCardProvider")]
        public object GiftCardProvider { get; set; }

        [JsonProperty("giftCardAsDiscount")]
        [JsonPropertyName("giftCardAsDiscount")]
        public object GiftCardAsDiscount { get; set; }

        [JsonProperty("koinUrl")]
        [JsonPropertyName("koinUrl")]
        public object KoinUrl { get; set; }

        [JsonProperty("accountId")]
        [JsonPropertyName("accountId")]
        public string AccountId { get; set; }

        [JsonProperty("parentAccountId")]
        [JsonPropertyName("parentAccountId")]
        public object ParentAccountId { get; set; }

        [JsonProperty("bankIssuedInvoiceIdentificationNumber")]
        [JsonPropertyName("bankIssuedInvoiceIdentificationNumber")]
        public object BankIssuedInvoiceIdentificationNumber { get; set; }

        [JsonProperty("bankIssuedInvoiceIdentificationNumberFormatted")]
        [JsonPropertyName("bankIssuedInvoiceIdentificationNumberFormatted")]
        public object BankIssuedInvoiceIdentificationNumberFormatted { get; set; }

        [JsonProperty("bankIssuedInvoiceBarCodeNumber")]
        [JsonPropertyName("bankIssuedInvoiceBarCodeNumber")]
        public object BankIssuedInvoiceBarCodeNumber { get; set; }

        [JsonProperty("bankIssuedInvoiceBarCodeType")]
        [JsonPropertyName("bankIssuedInvoiceBarCodeType")]
        public object BankIssuedInvoiceBarCodeType { get; set; }

        [JsonProperty("billingAddress")]
        [JsonPropertyName("billingAddress")]
        public BillingAddress BillingAddress { get; set; }
    }

    public class PaymentData
    {
        [JsonProperty("giftCards")]
        [JsonPropertyName("giftCards")]
        public List<object> GiftCards { get; set; }

        [JsonProperty("transactions")]
        [JsonPropertyName("transactions")]
        public List<Transaction> Transactions { get; set; }
    }

    public class PickupStoreInfo
    {
        [JsonProperty("additionalInfo")]
        [JsonPropertyName("additionalInfo")]
        public object AdditionalInfo { get; set; }

        [JsonProperty("address")]
        [JsonPropertyName("address")]
        public object Address { get; set; }

        [JsonProperty("dockId")]
        [JsonPropertyName("dockId")]
        public object DockId { get; set; }

        [JsonProperty("friendlyName")]
        [JsonPropertyName("friendlyName")]
        public object FriendlyName { get; set; }

        [JsonProperty("isPickupStore")]
        [JsonPropertyName("isPickupStore")]
        public bool IsPickupStore { get; set; }
    }

    public class PriceDefinition
    {
        [JsonProperty("sellingPrices")]
        [JsonPropertyName("sellingPrices")]
        public List<SellingPrice> SellingPrices { get; set; }

        [JsonProperty("calculatedSellingPrice")]
        [JsonPropertyName("calculatedSellingPrice")]
        public int CalculatedSellingPrice { get; set; }

        [JsonProperty("total")]
        [JsonPropertyName("total")]
        public int Total { get; set; }
    }

    public class RatesAndBenefitsData
    {
        [JsonProperty("id")]
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonProperty("rateAndBenefitsIdentifiers")]
        [JsonPropertyName("rateAndBenefitsIdentifiers")]
        public List<object> RateAndBenefitsIdentifiers { get; set; }
    }

    public class Restitutions
    {
    }

    public class SelectedAddress
    {
        [JsonProperty("addressId")]
        [JsonPropertyName("addressId")]
        public string AddressId { get; set; }

        [JsonProperty("versionId")]
        [JsonPropertyName("versionId")]
        public object VersionId { get; set; }

        [JsonProperty("entityId")]
        [JsonPropertyName("entityId")]
        public object EntityId { get; set; }

        [JsonProperty("addressType")]
        [JsonPropertyName("addressType")]
        public string AddressType { get; set; }

        [JsonProperty("receiverName")]
        [JsonPropertyName("receiverName")]
        public string ReceiverName { get; set; }

        [JsonProperty("street")]
        [JsonPropertyName("street")]
        public string Street { get; set; }

        [JsonProperty("number")]
        [JsonPropertyName("number")]
        public string Number { get; set; }

        [JsonProperty("complement")]
        [JsonPropertyName("complement")]
        public string Complement { get; set; }

        [JsonProperty("neighborhood")]
        [JsonPropertyName("neighborhood")]
        public string Neighborhood { get; set; }

        [JsonProperty("postalCode")]
        [JsonPropertyName("postalCode")]
        public string PostalCode { get; set; }

        [JsonProperty("city")]
        [JsonPropertyName("city")]
        public string City { get; set; }

        [JsonProperty("state")]
        [JsonPropertyName("state")]
        public string State { get; set; }

        [JsonProperty("country")]
        [JsonPropertyName("country")]
        public string Country { get; set; }

        [JsonProperty("reference")]
        [JsonPropertyName("reference")]
        public object Reference { get; set; }

        [JsonProperty("geoCoordinates")]
        [JsonPropertyName("geoCoordinates")]
        public List<double> GeoCoordinates { get; set; }
    }

    public class Seller
    {
        [JsonProperty("id")]
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonProperty("logo")]
        [JsonPropertyName("logo")]
        public string Logo { get; set; }

        [JsonProperty("fulfillmentEndpoint")]
        [JsonPropertyName("fulfillmentEndpoint")]
        public string FulfillmentEndpoint { get; set; }
    }

    public class SellingPrice
    {
        [JsonProperty("value")]
        [JsonPropertyName("value")]
        public int Value { get; set; }

        [JsonProperty("quantity")]
        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }
    }

    public class ShippingData
    {
        [JsonProperty("id")]
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonProperty("address")]
        [JsonPropertyName("address")]
        public Address Address { get; set; }

        [JsonProperty("logisticsInfo")]
        [JsonPropertyName("logisticsInfo")]
        public List<LogisticsInfo> LogisticsInfo { get; set; }

        [JsonProperty("trackingHints")]
        [JsonPropertyName("trackingHints")]
        public object TrackingHints { get; set; }

        [JsonProperty("selectedAddresses")]
        [JsonPropertyName("selectedAddresses")]
        public List<SelectedAddress> SelectedAddresses { get; set; }
    }

    public class Sla
    {
        [JsonProperty("id")]
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonProperty("shippingEstimate")]
        [JsonPropertyName("shippingEstimate")]
        public string ShippingEstimate { get; set; }

        [JsonProperty("deliveryWindow")]
        [JsonPropertyName("deliveryWindow")]
        public object DeliveryWindow { get; set; }

        [JsonProperty("price")]
        [JsonPropertyName("price")]
        public int Price { get; set; }

        [JsonProperty("deliveryChannel")]
        [JsonPropertyName("deliveryChannel")]
        public string DeliveryChannel { get; set; }

        [JsonProperty("pickupStoreInfo")]
        [JsonPropertyName("pickupStoreInfo")]
        public PickupStoreInfo PickupStoreInfo { get; set; }

        [JsonProperty("polygonName")]
        [JsonPropertyName("polygonName")]
        public object PolygonName { get; set; }

        [JsonProperty("lockTTL")]
        [JsonPropertyName("lockTTL")]
        public string LockTTL { get; set; }

        [JsonProperty("pickupPointId")]
        [JsonPropertyName("pickupPointId")]
        public object PickupPointId { get; set; }

        [JsonProperty("transitTime")]
        [JsonPropertyName("transitTime")]
        public object TransitTime { get; set; }

        [JsonProperty("pickupDistance")]
        [JsonPropertyName("pickupDistance")]
        public object PickupDistance { get; set; }
    }

    public class StorePreferencesData
    {
        [JsonProperty("countryCode")]
        [JsonPropertyName("countryCode")]
        public string CountryCode { get; set; }

        [JsonProperty("currencyCode")]
        [JsonPropertyName("currencyCode")]
        public string CurrencyCode { get; set; }

        [JsonProperty("currencyFormatInfo")]
        [JsonPropertyName("currencyFormatInfo")]
        public CurrencyFormatInfo CurrencyFormatInfo { get; set; }

        [JsonProperty("currencyLocale")]
        [JsonPropertyName("currencyLocale")]
        public int CurrencyLocale { get; set; }

        [JsonProperty("currencySymbol")]
        [JsonPropertyName("currencySymbol")]
        public string CurrencySymbol { get; set; }

        [JsonProperty("timeZone")]
        [JsonPropertyName("timeZone")]
        public string TimeZone { get; set; }
    }

    public class Total
    {
        [JsonProperty("id")]
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonProperty("value")]
        [JsonPropertyName("value")]
        public int Value { get; set; }
    }

    public class Transaction
    {
        [JsonProperty("isActive")]
        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; }

        [JsonProperty("transactionId")]
        [JsonPropertyName("transactionId")]
        public string TransactionId { get; set; }

        [JsonProperty("merchantName")]
        [JsonPropertyName("merchantName")]
        public string MerchantName { get; set; }

        [JsonProperty("payments")]
        [JsonPropertyName("payments")]
        public List<Payment> Payments { get; set; }
    }
}

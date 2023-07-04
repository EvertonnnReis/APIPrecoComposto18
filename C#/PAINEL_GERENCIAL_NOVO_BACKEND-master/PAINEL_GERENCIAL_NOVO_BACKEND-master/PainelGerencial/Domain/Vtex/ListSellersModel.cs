using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PainelGerencial.Domain.Vtex
{
    public class ListSellersModel
    {
        [JsonProperty("paging")]
        [JsonPropertyName("paging")]
        public Paging Paging { get; set; }

        [JsonProperty("items")]
        [JsonPropertyName("items")]
        public List<SellerItem> Items { get; set; }
    }
    public class AvailableSalesChannel
    {
        [JsonProperty("id")]
        [JsonPropertyName("id")]
        public int Id { get; set; }
    }

    public class Group
    {
        [JsonProperty("id")]
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonProperty("activeSellers")]
        [JsonPropertyName("activeSellers")]
        public int ActiveSellers { get; set; }

        [JsonProperty("inactiveSellers")]
        [JsonPropertyName("inactiveSellers")]
        public int InactiveSellers { get; set; }
    }

    public class SellerItem
    {
        [JsonProperty("id")]
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonProperty("logo")]
        [JsonPropertyName("logo")]
        public string Logo { get; set; }

        [JsonProperty("taxCode")]
        [JsonPropertyName("taxCode")]
        public string TaxCode { get; set; }

        [JsonProperty("email")]
        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonProperty("description")]
        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonProperty("sellerCommissionConfiguration")]
        [JsonPropertyName("sellerCommissionConfiguration")]
        public object SellerCommissionConfiguration { get; set; }

        [JsonProperty("catalogSystemEndpoint")]
        [JsonPropertyName("catalogSystemEndpoint")]
        public string CatalogSystemEndpoint { get; set; }

        [JsonProperty("CSCIdentification")]
        [JsonPropertyName("CSCIdentification")]
        public string CSCIdentification { get; set; }

        [JsonProperty("account")]
        [JsonPropertyName("account")]
        public string Account { get; set; }

        [JsonProperty("channel")]
        [JsonPropertyName("channel")]
        public string Channel { get; set; }

        [JsonProperty("salesChannel")]
        [JsonPropertyName("salesChannel")]
        public string SalesChannel { get; set; }

        [JsonProperty("score")]
        [JsonPropertyName("score")]
        public double Score { get; set; }

        [JsonProperty("exchangeReturnPolicy")]
        [JsonPropertyName("exchangeReturnPolicy")]
        public string ExchangeReturnPolicy { get; set; }

        [JsonProperty("deliveryPolicy")]
        [JsonPropertyName("deliveryPolicy")]
        public string DeliveryPolicy { get; set; }

        [JsonProperty("securityPrivacyPolicy")]
        [JsonPropertyName("securityPrivacyPolicy")]
        public object SecurityPrivacyPolicy { get; set; }

        [JsonProperty("fulfillmentSellerId")]
        [JsonPropertyName("fulfillmentSellerId")]
        public string FulfillmentSellerId { get; set; }

        [JsonProperty("user")]
        [JsonPropertyName("user")]
        public string User { get; set; }

        [JsonProperty("password")]
        [JsonPropertyName("password")]
        public string Password { get; set; }

        [JsonProperty("groups")]
        [JsonPropertyName("groups")]
        public List<Group> Groups { get; set; }

        [JsonProperty("integration")]
        [JsonPropertyName("integration")]
        public string Integration { get; set; }

        [JsonProperty("offers")]
        [JsonPropertyName("offers")]
        public Offers Offers { get; set; }

        [JsonProperty("name")]
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonProperty("isActive")]
        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; }

        [JsonProperty("fulfillmentEndpoint")]
        [JsonPropertyName("fulfillmentEndpoint")]
        public string FulfillmentEndpoint { get; set; }

        [JsonProperty("allowHybridPayments")]
        [JsonPropertyName("allowHybridPayments")]
        public bool AllowHybridPayments { get; set; }

        [JsonProperty("isBetterScope")]
        [JsonPropertyName("isBetterScope")]
        public bool IsBetterScope { get; set; }

        [JsonProperty("sellerType")]
        [JsonPropertyName("sellerType")]
        public int SellerType { get; set; }

        [JsonProperty("availableSalesChannels")]
        [JsonPropertyName("availableSalesChannels")]
        public List<AvailableSalesChannel> AvailableSalesChannels { get; set; }

        [JsonProperty("isVtex")]
        [JsonPropertyName("isVtex")]
        public bool IsVtex { get; set; }

        [JsonProperty("trustPolicy")]
        [JsonPropertyName("trustPolicy")]
        public string TrustPolicy { get; set; }
    }

    public class Offers
    {
        [JsonProperty("pending")]
        [JsonPropertyName("pending")]
        public int Pending { get; set; }
    }

    public class Paging
    {
        [JsonProperty("from")]
        [JsonPropertyName("from")]
        public int From { get; set; }

        [JsonProperty("to")]
        [JsonPropertyName("to")]
        public int To { get; set; }

        [JsonProperty("total")]
        [JsonPropertyName("total")]
        public int Total { get; set; }
    }


}

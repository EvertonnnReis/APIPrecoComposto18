using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using System.Text.Json.Serialization;
using System.Collections;

namespace PainelGerencial.Domain.Vtex
{
    public class PedidosSellerListModel
    {
        [JsonProperty("list")]
        [JsonPropertyName("list")]
        public List<List> List { get; set; }

        [JsonProperty("facets")]
        [JsonPropertyName("facets")]
        public List<object> Facets { get; set; }

        [JsonProperty("paging")]
        [JsonPropertyName("paging")]
        public PagingSeller Paging { get; set; }

        [JsonProperty("stats")]
        [JsonPropertyName("stats")]
        public StatsExterno Stats { get; set; }

        [JsonProperty("reportRecordsLimit")]
        [JsonPropertyName("reportRecordsLimit")]
        public int ReportRecordsLimit { get; set; }
    }
    public class Facets
    {
    }

    public class List
    {
        [JsonProperty("orderId")]
        [JsonPropertyName("orderId")]
        public string OrderId { get; set; }

        [JsonProperty("creationDate")]
        [JsonPropertyName("creationDate")]
        public DateTime CreationDate { get; set; }

        [JsonProperty("clientName")]
        [JsonPropertyName("clientName")]
        public string ClientName { get; set; }

        [JsonProperty("items")]
        [JsonPropertyName("items")]
        public object Items { get; set; }

        [JsonProperty("totalValue")]
        [JsonPropertyName("totalValue")]
        public double TotalValue { get; set; }

        [JsonProperty("paymentNames")]
        [JsonPropertyName("paymentNames")]
        public string PaymentNames { get; set; }

        [JsonProperty("status")]
        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonProperty("statusDescription")]
        [JsonPropertyName("statusDescription")]
        public string StatusDescription { get; set; }

        [JsonProperty("marketPlaceOrderId")]
        [JsonPropertyName("marketPlaceOrderId")]
        public string MarketPlaceOrderId { get; set; }

        [JsonProperty("sequence")]
        [JsonPropertyName("sequence")]
        public string Sequence { get; set; }

        [JsonProperty("salesChannel")]
        [JsonPropertyName("salesChannel")]
        public string SalesChannel { get; set; }

        [JsonProperty("affiliateId")]
        [JsonPropertyName("affiliateId")]
        public string AffiliateId { get; set; }

        [JsonProperty("origin")]
        [JsonPropertyName("origin")]
        public string Origin { get; set; }

        [JsonProperty("workflowInErrorState")]
        [JsonPropertyName("workflowInErrorState")]
        public bool WorkflowInErrorState { get; set; }

        [JsonProperty("workflowInRetry")]
        [JsonPropertyName("workflowInRetry")]
        public bool WorkflowInRetry { get; set; }

        [JsonProperty("lastMessageUnread")]
        [JsonPropertyName("lastMessageUnread")]
        public string LastMessageUnread { get; set; }

        [JsonProperty("ShippingEstimatedDate")]
        [JsonPropertyName("ShippingEstimatedDate")]
        public DateTime? ShippingEstimatedDate { get; set; }

        [JsonProperty("ShippingEstimatedDateMax")]
        [JsonPropertyName("ShippingEstimatedDateMax")]
        public DateTime? ShippingEstimatedDateMax { get; set; }

        [JsonProperty("ShippingEstimatedDateMin")]
        [JsonPropertyName("ShippingEstimatedDateMin")]
        public DateTime? ShippingEstimatedDateMin { get; set; }

        [JsonProperty("orderIsComplete")]
        [JsonPropertyName("orderIsComplete")]
        public bool OrderIsComplete { get; set; }

        [JsonProperty("listId")]
        [JsonPropertyName("listId")]
        public object ListId { get; set; }

        [JsonProperty("listType")]
        [JsonPropertyName("listType")]
        public object ListType { get; set; }

        [JsonProperty("authorizedDate")]
        [JsonPropertyName("authorizedDate")]
        public DateTime? AuthorizedDate { get; set; }

        [JsonProperty("callCenterOperatorName")]
        [JsonPropertyName("callCenterOperatorName")]
        public object CallCenterOperatorName { get; set; }

        [JsonProperty("totalItems")]
        [JsonPropertyName("totalItems")]
        public int TotalItems { get; set; }

        [JsonProperty("currencyCode")]
        [JsonPropertyName("currencyCode")]
        public string CurrencyCode { get; set; }

        [JsonProperty("hostname")]
        [JsonPropertyName("hostname")]
        public string Hostname { get; set; }

        [JsonProperty("invoiceOutput")]
        [JsonPropertyName("invoiceOutput")]
        public List<string> InvoiceOutput { get; set; }

        [JsonProperty("invoiceInput")]
        [JsonPropertyName("invoiceInput")]
        public object InvoiceInput { get; set; }

        [JsonProperty("lastChange")]
        [JsonPropertyName("lastChange")]
        public DateTime LastChange { get; set; }

        [JsonProperty("isAllDelivered")]
        [JsonPropertyName("isAllDelivered")]
        public bool IsAllDelivered { get; set; }

        [JsonProperty("isAnyDelivered")]
        [JsonPropertyName("isAnyDelivered")]
        public bool IsAnyDelivered { get; set; }

        [JsonProperty("giftCardProviders")]
        [JsonPropertyName("giftCardProviders")]
        public object GiftCardProviders { get; set; }

        [JsonProperty("orderFormId")]
        [JsonPropertyName("orderFormId")]
        public string OrderFormId { get; set; }

        [JsonProperty("paymentApprovedDate")]
        [JsonPropertyName("paymentApprovedDate")]
        public DateTime? PaymentApprovedDate { get; set; }

        [JsonProperty("readyForHandlingDate")]
        [JsonPropertyName("readyForHandlingDate")]
        public DateTime? ReadyForHandlingDate { get; set; }

        [JsonProperty("deliveryDates")]
        [JsonPropertyName("deliveryDates")]
        public object DeliveryDates { get; set; }
    }

    public class PagingSeller
    {
        [JsonProperty("total")]
        [JsonPropertyName("total")]
        public int Total { get; set; }

        [JsonProperty("pages")]
        [JsonPropertyName("pages")]
        public int Pages { get; set; }

        [JsonProperty("currentPage")]
        [JsonPropertyName("currentPage")]
        public int CurrentPage { get; set; }

        [JsonProperty("perPage")]
        [JsonPropertyName("perPage")]
        public int PerPage { get; set; }
    }

    public class StatsExterno
    {
        [JsonProperty("stats")]
        [JsonPropertyName("stats")]
        public StatsInterno Stats { get; set; }
    }

    public class StatsInterno
    {
        [JsonProperty("totalValue")]
        [JsonPropertyName("totalValue")]
        public TotalValue TotalValue { get; set; }

        [JsonProperty("totalItems")]
        [JsonPropertyName("totalItems")]
        public TotalItems TotalItems { get; set; }
    }

    public class TotalItems
    {
        [JsonProperty("Count")]
        [JsonPropertyName("Count")]
        public int Count { get; set; }

        [JsonProperty("Max")]
        [JsonPropertyName("Max")]
        public double Max { get; set; }

        [JsonProperty("Mean")]
        [JsonPropertyName("Mean")]
        public double Mean { get; set; }

        [JsonProperty("Min")]
        [JsonPropertyName("Min")]
        public double Min { get; set; }

        [JsonProperty("Missing")]
        [JsonPropertyName("Missing")]
        public int Missing { get; set; }

        [JsonProperty("StdDev")]
        [JsonPropertyName("StdDev")]
        public double StdDev { get; set; }

        [JsonProperty("Sum")]
        [JsonPropertyName("Sum")]
        public double Sum { get; set; }

        [JsonProperty("SumOfSquares")]
        [JsonPropertyName("SumOfSquares")]
        public double SumOfSquares { get; set; }

        [JsonProperty("Facets")]
        [JsonPropertyName("Facets")]
        public Facets Facets { get; set; }
    }

    public class TotalValue
    {
        [JsonProperty("Count")]
        [JsonPropertyName("Count")]
        public int Count { get; set; }

        [JsonProperty("Max")]
        [JsonPropertyName("Max")]
        public double Max { get; set; }

        [JsonProperty("Mean")]
        [JsonPropertyName("Mean")]
        public double Mean { get; set; }

        [JsonProperty("Min")]
        [JsonPropertyName("Min")]
        public double Min { get; set; }

        [JsonProperty("Missing")]
        [JsonPropertyName("Missing")]
        public int Missing { get; set; }

        [JsonProperty("StdDev")]
        [JsonPropertyName("StdDev")]
        public double StdDev { get; set; }

        [JsonProperty("Sum")]
        [JsonPropertyName("Sum")]
        public double Sum { get; set; }

        [JsonProperty("SumOfSquares")]
        [JsonPropertyName("SumOfSquares")]
        public double SumOfSquares { get; set; }

        [JsonProperty("Facets")]
        [JsonPropertyName("Facets")]
        public Facets Facets { get; set; }
    }


}

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Exito.Integracion.TurboCarulla.OrderInvoice.Models.Sap.Poblaciones
{
	public class RequestData : Base
	{
		[Required][JsonProperty("daneCode")] public int DaneCode { get; set; }
	}

	[BsonIgnoreExtraElements]
	public class ResponseData : Base
	{
		[JsonProperty("daneCode")][BsonElement("codigoDane")] public int DaneCode { get; set; }
		[JsonProperty("city")][BsonElement("ciudad")] public string City { get; set; }
		[JsonProperty("region")][BsonElement("region")] public int Region { get; set; }
		[JsonProperty("country")][BsonElement("ps")] public string Country { get; set; }
		[JsonProperty("language")][BsonElement("ci")] public string Language { get; set; }
		[BsonIgnore] public override string TraceId { get; set; }
	}
}

namespace Exito.Integracion.TurboCarulla.OrderInvoice.Models.Sap.InvoicePending
{
	[BsonIgnoreExtraElements]
	public class RequestData
	{
		[JsonProperty("id")][BsonElement("_id")][BsonId] public ObjectId Id { get; set; }
		[JsonProperty("idPedido")][BsonElement("idPedido")] public string OrderId { get; set; }
		[JsonProperty("origen")][BsonElement("origen")] public string Origin { get; set; }
		[JsonProperty("invoiceStatus")][BsonElement("invoiceStatus")] public string Status { get; set; }
		[JsonProperty("indicadorPedido")][BsonElement("indicadorPedido")] public string OrderPrefix { get; set; }
		[JsonProperty("fechaCompra")][BsonElement("fechaCompra")] public string OrderDate { get; set; }
		[JsonProperty("horaCompra")][BsonElement("horaCompra")] public string OrderTime { get; set; }
		[JsonProperty("idDependencia")][BsonElement("idDependencia")] public string LocationId { get; set; }
		[JsonProperty("infoCliente")][BsonElement("infoCliente")] public CustomerType Customer { get; set; }
		[JsonProperty("infoProductos")][BsonElement("infoProductos")] public ProductType[] Products { get; set; }
		[JsonProperty("infoTransaccion")][BsonElement("infoTransaccion")] public TransactionType Transaccion { get; set; }
		[JsonProperty("idTransaccionSinco")][BsonElement("idTransaccionSinco")] public string SincoId { get; set; }
		[JsonProperty("business")][BsonElement("business")] public string Business { get; set; }
		[JsonProperty("typeBus")][BsonElement("typeBus")] public string TypeBus { get; set; }
		[JsonProperty("burks")][BsonElement("burks")] public string Burks { get; set; }
		[JsonProperty("vstel")][BsonElement("vstel")] public string Vstel { get; set; }
		[JsonProperty("country")][BsonElement("country")] public string Country { get; set; }
		[JsonProperty("waers")][BsonElement("waers")] public string Waers { get; set; }
		[JsonProperty("personaNatural")][BsonElement("personaNatural")] public string PersonType { get; set; }
		[JsonProperty("flete")][BsonElement("flete")] public int Freight { get; set; }
		[JsonProperty("seguros")][BsonElement("seguros")] public int Insurance { get; set; }
		[JsonProperty("otros")][BsonElement("otros")] public int OthersCost { get; set; }
		[JsonProperty("consecutivo")][BsonElement("consecutivo")] public int Index { get; set; }
		[JsonProperty("invoiceRetry")][BsonElement("invoiceRetry")] public int InvoiceRetry { get; set; }
		[JsonProperty("traceId")][BsonElement("traceId")] public string TraceId { get; set; }
	}

	public class ResponseData : Base
	{
		[JsonProperty("isDiscarted")] public bool IsDiscarted { get; set; }
		[JsonProperty("message")] public string Message { get; set; }
	}

	[BsonIgnoreExtraElements]
	public class CustomerType
	{
		[JsonProperty("documento")][BsonElement("documento")] public string DocumentNumber { get; set; }
		[JsonProperty("tipoDocumento")][BsonElement("tipoDocumento")] public int DocumentType { get; set; }
		[JsonProperty("nombre")][BsonElement("nombre")] public string Name { get; set; }
		[JsonProperty("email")][BsonElement("email")] public string Email { get; set; }
		[JsonProperty("telefono")][BsonElement("telefono")] public string PhoneNumber { get; set; }
		[JsonProperty("direccion")][BsonElement("direccion")] public string Address { get; set; }
		[JsonProperty("ciudad")][BsonElement("ciudad")] public string City { get; set; }
		[JsonProperty("region")][BsonElement("region")] public int Region { get; set; }
	}

	[BsonIgnoreExtraElements]
	public class TransactionType
	{
		[JsonProperty("idTransaccion")][BsonElement("idTransaccion")] public string Id { get; set; }
		[JsonProperty("autorizacion")][BsonElement("autorizacion")] public string Auth { get; set; }
		[JsonProperty("binNumero")][BsonElement("binNumero")] public int Bin { get; set; }
		[JsonProperty("tipoMedioPago")][BsonElement("tipoMedioPago")] public int PaymentMethod { get; set; }
		[JsonProperty("plazo")][BsonElement("plazo")] public int Dues { get; set; }
		[JsonProperty("valorTransaccion")][BsonElement("valorTransaccion")] public int Value { get; set; }
		[JsonProperty("cantidadPuntos")][BsonElement("cantidadPuntos")] public int Points { get; set; }
	}

	[BsonIgnoreExtraElements]
	public class ProductType
	{
		[JsonProperty("plu")][BsonElement("plu")] public string Plu { get; set; }
		[JsonProperty("descripcion")][BsonElement("descripcion")] public string Description { get; set; }
		[JsonProperty("cantidad")][BsonElement("cantidad")] public float Quantity { get; set; }
		[JsonProperty("valorVenta")][BsonElement("valorVenta")] public string Price { get; set; }
		[JsonProperty("iva")][BsonElement("iva")] public float IvaRate { get; set; }
		[JsonProperty("secuencia")][BsonElement("secuencia")] public float Sequence { get; set; }
		[JsonProperty("matnr")][BsonElement("matnr")] public string MaterialCode { get; set; }
		[JsonProperty("posrnNumero")][BsonElement("posrnNumero")] public int PosNumber { get; set; }
		[JsonProperty("sublineaRetail")][BsonElement("sublineaRetail")] public int SubLine { get; set; }
		[JsonProperty("signo")][BsonElement("signo")] public string Sign { get; set; }
		[JsonProperty("valorDescuento")][BsonElement("valorDescuento")] public string Discout { get; set; }
	}
}

namespace Exito.Integracion.TurboCarulla.OrderInvoice.Models.Sap.InvoiceCollect
{
	public class RequestData : Base
	{
		[JsonProperty("orderRegisterId", Order = 1)] public int OrderRegisterId { get; set; }
		[JsonProperty("orderId", Order = 2)] public int OrderId { get; set; }
		[JsonProperty("customerCity", Order = 3)] public string CustomerCity { get; set; }
		[JsonProperty("customerID", Order = 4)] public string CustomerID { get; set; }
		[JsonProperty("customerName", Order = 5)] public string CustomerName { get; set; }
		[JsonProperty("customerAddress", Order = 6)] public string CustomerAddress { get; set; }
		[JsonProperty("customerPhone", Order = 7)] public string CustomerPhone { get; set; }
		[JsonProperty("orderNumber", Order = 8)] public string OrderNumber { get; set; }
		[JsonProperty("businessSubType", Order = 9)] public string BusinessSubType { get; set; }
		[JsonProperty("sellerId", Order = 10)] public int SellerId { get; set; }
		[JsonProperty("referenceBonus", Order = 11)] public string ReferenceBonus { get; set; }
		[JsonProperty("email", Order = 12)] public string Email { get; set; }
		[JsonProperty("collectItems", Order = 13)] public List<PaymetType> CollectItems { get; set; }
		[JsonProperty("debtorNumber", Order = 14)] public string DebtorNumber { get; set; }
		[JsonProperty("region", Order = 15)] public string Region { get; set; }
		[JsonProperty("exchangRate", Order = 16)] public int ExchangRate { get; set; }
		[JsonProperty("business", Order = 17)] public string Business { get; set; }
		[JsonProperty("previousDocumentNumber", Order = 18)] public int PreviousDocumentNumber { get; set; }
		[JsonProperty("marketPlaceOrderMovementID", Order = 19)] public int MarketPlaceOrderMovementID { get; set; }
		[JsonProperty("financialSociety", Order = 20)] public string FinancialSociety { get; set; }
		[JsonProperty("countryCode", Order = 21)] public string CountryCode { get; set; }
		[JsonProperty("currencyCode", Order = 22)] public string CurrencyCode { get; set; }
		[JsonProperty("collectType", Order = 23)] public List<int> CollectType { get; set; }
		[JsonProperty("processTransactionId", Order = 24)] public string ProcessTransactionId { get; set; }
		[JsonProperty("plus", Order = 25)] public List<PluType> Plus { get; set; }
		[JsonProperty("traceId", Order = 26)] public override string TraceId { get; set; }
	}

	public class PaymetType
	{
		[JsonProperty("authorizationNumber", Order = 1)] public string AuthorizationNumber { get; set; }
		[JsonProperty("transactionId", Order = 2)] public string TransactionId { get; set; }
		[JsonProperty("paymentTypeCode", Order = 3)] public string PaymentTypeCode { get; set; }
		[JsonProperty("amount", Order = 4)] public long Amount { get; set; }
		[JsonProperty("bond", Order = 5)] public string Bond { get; set; }
		[JsonProperty("nit", Order = 6)] public string Nit { get; set; }
		[JsonProperty("pointsRedeemed", Order = 7)] public int PointsRedeemed { get; set; }
	}

	public class PluType
	{
		[JsonProperty("orderLineId", Order = 1)] public string OrderLineId { get; set; }
		[JsonProperty("plu", Order = 2)] public string Plu { get; set; }
	}
}

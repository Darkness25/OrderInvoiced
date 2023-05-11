using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace Exito.Integracion.TurboCarulla.OrderInvoice.Models
{
	public class Base
	{
		[JsonProperty("traceId", NullValueHandling = NullValueHandling.Ignore)] public virtual string TraceId { get; set; }
		[JsonProperty("retryTimes")] public int RetryTimes { get; set; }
		[JsonProperty("transactionDateTime")] public DateTime TransactionDateTime { get; set; }
	}

	public class DiscartType
	{
		[Required][JsonProperty("locationID")] public string LocationID { get; set; }
		[Required][JsonProperty("orderID")] public string OrderID { get; set; }
		[Required][JsonProperty("traceId")] public string TraceId { get; set; }
		[Required][JsonProperty("typeId")] public string TypeID { get; set; }
		[Required][JsonProperty("reason")] public string Reason { get; set; }
		[Required][JsonProperty("isProcessed")] public bool IsProcessed { get; set; }
		[Required][JsonProperty("dateCreated")] public DateTime DateCreated { get; set; }
		[JsonIgnore] public bool IsValid { get; set; }
	}
}

namespace Exito.Integracion.TurboCarulla.OrderInvoice.Models.OrderValidated
{
	public class ResponseData : Base
	{
		[JsonProperty("response", NullValueHandling = NullValueHandling.Ignore)] public bool Response { get; set; }
		[JsonProperty("message", NullValueHandling = NullValueHandling.Ignore)] public string Message { get; set; }
		[JsonProperty("messageDetail", NullValueHandling = NullValueHandling.Ignore)] public string MessageDetail { get; set; }
		[JsonProperty("orderData", NullValueHandling = NullValueHandling.Ignore)] public OrderDataType OrderData { get; set; }
	}

	public class OrderDataType
	{
		[JsonProperty("infoGeneral", NullValueHandling = NullValueHandling.Ignore)] public InfoGeneralType InfoGeneral { get; set; }
		[JsonProperty("infoCliente", NullValueHandling = NullValueHandling.Ignore)] public InfoClienteType InfoCliente { get; set; }
		[JsonProperty("infoProducto", NullValueHandling = NullValueHandling.Ignore)] public InfoProductoType[] InfoProducto { get; set; }
		[JsonProperty("infoTransaccion", NullValueHandling = NullValueHandling.Ignore)] public InfoTransaccionType InfoTransaccion { get; set; }
		[JsonProperty("infoMedioPago", NullValueHandling = NullValueHandling.Ignore)] public InfoMedioPagoType InfoMedioPago { get; set; }
		[JsonProperty("infoDevuelta", NullValueHandling = NullValueHandling.Ignore)] public InfoDevueltaType InfoDevuelta { get; set; }
		[JsonProperty("observacion", NullValueHandling = NullValueHandling.Ignore)] public string Observacion { get; set; }
		[JsonProperty("opcional", NullValueHandling = NullValueHandling.Ignore)] public string Opcional { get; set; }
		[JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)] public string TraceId { get; set; }
	}

	public class InfoGeneralType
	{
		[JsonProperty("idPedido", NullValueHandling = NullValueHandling.Ignore)] public string IdPedido { get; set; }
		[JsonProperty("idOrderPos", NullValueHandling = NullValueHandling.Ignore)] public string IdOrderPos { get; set; }
		[JsonProperty("fechaCompra", NullValueHandling = NullValueHandling.Ignore)] public string FechaCompra { get; set; }
		[JsonProperty("horaCompra", NullValueHandling = NullValueHandling.Ignore)] public string HoraCompra { get; set; }
		[JsonProperty("idDependencia", NullValueHandling = NullValueHandling.Ignore)] public int IdDependencia { get; set; }
		[JsonProperty("tipoPedido", NullValueHandling = NullValueHandling.Ignore)] public string TipoPedido { get; set; }
		[JsonProperty("estadoPOS", NullValueHandling = NullValueHandling.Ignore)] public string EstadoPOS { get; set; }
		[JsonProperty("tipoIncremento", NullValueHandling = NullValueHandling.Ignore)] public string TipoIncremento { get; set; }
		[JsonProperty("porcentajeIncremento", NullValueHandling = NullValueHandling.Ignore)] public string PorcentajeIncremento { get; set; }
		[JsonProperty("tieneIncremento", NullValueHandling = NullValueHandling.Ignore)] public string TieneIncremento { get; set; }
		[JsonProperty("pagoPuntos", NullValueHandling = NullValueHandling.Ignore)] public string PagoPuntos { get; set; }
		[JsonProperty("numeroPedidoGlobal", NullValueHandling = NullValueHandling.Ignore)] public string NumeroPedidoGlobal { get; set; }
		[JsonProperty("puntosMaximosSubpedido", NullValueHandling = NullValueHandling.Ignore)] public string PuntosMaximosSubpedido { get; set; }
		[JsonProperty("domicilios", NullValueHandling = NullValueHandling.Ignore)] public string Domicilios { get; set; }
		[JsonProperty("clavePos", NullValueHandling = NullValueHandling.Ignore)] public string ClavePos { get; set; }
		[JsonProperty("token", NullValueHandling = NullValueHandling.Ignore)] public string Token { get; set; }
		[JsonProperty("usoFuturo1", NullValueHandling = NullValueHandling.Ignore)] public string UsoFuturo1 { get; set; }
		[JsonProperty("usoFuturo2", NullValueHandling = NullValueHandling.Ignore)] public string UsoFuturo2 { get; set; }
		[JsonProperty("usoFuturo3", NullValueHandling = NullValueHandling.Ignore)] public string UsoFuturo3 { get; set; }
	}

	public class InfoClienteType
	{
		[JsonProperty("cedula", NullValueHandling = NullValueHandling.Ignore)] public string NroDocumento { get; set; }
		[JsonProperty("tipoDocumento", NullValueHandling = NullValueHandling.Ignore)] public string TipoDocumento { get; set; }
		[JsonProperty("nombre", NullValueHandling = NullValueHandling.Ignore)] public string Nombre { get; set; }
		[JsonProperty("email", NullValueHandling = NullValueHandling.Ignore)] public string Email { get; set; }
		[JsonProperty("telefono", NullValueHandling = NullValueHandling.Ignore)] public string Telefono { get; set; }
		[JsonProperty("ciudad", NullValueHandling = NullValueHandling.Ignore)] public string Ciudad { get; set; }
		[JsonProperty("direccion", NullValueHandling = NullValueHandling.Ignore)] public string Direccion { get; set; }
		[JsonProperty("cantidadPuntos", NullValueHandling = NullValueHandling.Ignore)] public string CantidadPuntos { get; set; }
		[JsonProperty("codigoConvenio", NullValueHandling = NullValueHandling.Ignore)] public string CodigoConvenio { get; set; }
		[JsonProperty("cedulaConvenio", NullValueHandling = NullValueHandling.Ignore)] public string DocumentoConvenio { get; set; }
		[JsonProperty("codigoDsctoEmpleado", NullValueHandling = NullValueHandling.Ignore)] public string CodigoDsctoEmpleado { get; set; }
		[JsonProperty("usoFuturo1", NullValueHandling = NullValueHandling.Ignore)] public string UsoFuturo1 { get; set; }
		[JsonProperty("usoFuturo2", NullValueHandling = NullValueHandling.Ignore)] public string UsoFuturo2 { get; set; }
	}

	public class InfoProductoType
	{
		[JsonProperty("producto", NullValueHandling = NullValueHandling.Ignore)] public ProductoType Producto { get; set; }
		[JsonProperty("infoDescuento", NullValueHandling = NullValueHandling.Ignore)] public DescuentoType Descuento { get; set; }
	}

	public class ProductoType
	{
		[JsonProperty("plu", NullValueHandling = NullValueHandling.Ignore)] public string Plu { get; set; }
		[JsonProperty("cantidad", NullValueHandling = NullValueHandling.Ignore)] public decimal Cantidad { get; set; }
		[JsonProperty("valorVenta", NullValueHandling = NullValueHandling.Ignore)] public string ValorVenta { get; set; }
		[JsonProperty("porcentajeIncremento", NullValueHandling = NullValueHandling.Ignore)] public string PorcentajeIncremento { get; set; }
		[JsonProperty("descripcion", NullValueHandling = NullValueHandling.Ignore)] public string Descripcion { get; set; }
		[JsonProperty("iva", NullValueHandling = NullValueHandling.Ignore)] public int Iva { get; set; }
		[JsonProperty("materialCode")] public string MaterialCode { get; set; }
		[JsonProperty("secuencia")] public long Secuencia { get; set; }
		[JsonProperty("posrnNumero")] public int PosNumero { get; set; }
		[JsonProperty("sublineaRetail")] public int Sublinea { get; set; }
		[JsonProperty("signo")] public string Signo { get; set; }
		[JsonProperty("valorDescuento")] public string ValorDescuento { get; set; }
	}

	public class DescuentoType
	{
		[JsonProperty("tipoPromo", NullValueHandling = NullValueHandling.Ignore)] public string Tipo { get; set; }
		[JsonProperty("codSincoPromo", NullValueHandling = NullValueHandling.Ignore)] public string CodSinco { get; set; }
		[JsonProperty("gastoMargen", NullValueHandling = NullValueHandling.Ignore)] public string GastoMargen { get; set; }
		[JsonProperty("valorDescuento", NullValueHandling = NullValueHandling.Ignore)] public string ValorDescuento { get; set; }
	}

	public class InfoTransaccionType
	{
		[JsonProperty("valorTransaccion", NullValueHandling = NullValueHandling.Ignore)] public int ValorTransaccion { get; set; }
		[JsonProperty("usuFuturo1", NullValueHandling = NullValueHandling.Ignore)] public string UsuFuturo1 { get; set; }
		[JsonProperty("usuFuturo2", NullValueHandling = NullValueHandling.Ignore)] public string UsuFuturo2 { get; set; }
	}

	public class InfoMedioPagoType
	{
		[JsonProperty("tipoMedioPago", NullValueHandling = NullValueHandling.Ignore)] public string Tipo { get; set; }
		[JsonProperty("totalDctoMedioPago", NullValueHandling = NullValueHandling.Ignore)] public string TotalDescuento { get; set; }
		[JsonProperty("valorPago", NullValueHandling = NullValueHandling.Ignore)] public int ValorPago { get; set; }
		[JsonProperty("autorizacion", NullValueHandling = NullValueHandling.Ignore)] public string Autorizacion { get; set; }
		[JsonProperty("binIndicador", NullValueHandling = NullValueHandling.Ignore)] public string BinIndicador { get; set; }
		[JsonProperty("binNumero", NullValueHandling = NullValueHandling.Ignore)] public string BinNumero { get; set; }
		[JsonProperty("bolsillo", NullValueHandling = NullValueHandling.Ignore)] public string Bolsillo { get; set; }
		[JsonProperty("naturalezaBolsillo", NullValueHandling = NullValueHandling.Ignore)] public string NaturalezaBolsillo { get; set; }
		[JsonProperty("pagoObligatorio", NullValueHandling = NullValueHandling.Ignore)] public string PagoObligatorio { get; set; }
		[JsonProperty("equivalentePuntos", NullValueHandling = NullValueHandling.Ignore)] public string EquivalentePuntos { get; set; }
		[JsonProperty("imprimeBono", NullValueHandling = NullValueHandling.Ignore)] public string ImprimeBono { get; set; }
		[JsonProperty("descMedioPago", NullValueHandling = NullValueHandling.Ignore)] public string Descripcion { get; set; }
	}

	public class InfoDevueltaType
	{
		[JsonProperty("valorDevuelta", NullValueHandling = NullValueHandling.Ignore)] public string Valor { get; set; }
		[JsonProperty("impresionTalon", NullValueHandling = NullValueHandling.Ignore)] public string Impresion { get; set; }
	}
}


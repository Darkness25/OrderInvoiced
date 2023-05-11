using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Exito.Integracion.TurboCarulla.OrderInvoice.Models.Pim.Maestras
{
	public class RequestData : Base
	{
		[Required][JsonProperty("locationID")] public int LocationId { get; set; }
	}

	[BsonIgnoreExtraElements]
	public class ResponseData : Base
	{
		[JsonProperty("locationID")][BsonElement("codigoDepend")] public string LocationId { get; set; }
		[JsonProperty("city")][BsonElement("ciudad")] public string City { get; set; }
		[JsonProperty("daneCode")][BsonElement("codigoDaneCiudad")] public string DaneCode { get; set; }
		[JsonProperty("description")][BsonElement("descripcion")] public string Description { get; set; }
		[JsonProperty("descripcionCiudad")][BsonElement("descripcionCiudad")] public string CityName { get; set; }
		[JsonProperty("address")][BsonElement("direccion")] public string Address { get; set; }
		[JsonProperty("phone")][BsonElement("telefono")] public string Phone { get; set; }
		[BsonIgnore] public override string TraceId { get; set; }
	}
}
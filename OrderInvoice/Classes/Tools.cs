using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace Exito.Integracion.TurboCarulla.OrderInvoice
{
	public static class Tools
	{

		public static string FindInJson(string JsonMessage, string fieldName)
		{
			JObject jsonPayload;

			if (JsonMessage.StartsWith("["))
			{
				JArray array = JArray.Parse(JsonMessage);
				jsonPayload = JObject.Parse(array[0].ToString());
			}
			else jsonPayload = JObject.Parse(JsonMessage);

			string traceId = jsonPayload[fieldName].ToString();

			return traceId;
		}

		public static bool DocumentIsValid(string documentNumber)
		{
			bool result = documentNumber == "0" || documentNumber.ToString().Length < 3 || documentNumber.ToString().Length > 10 || Regex.IsMatch(documentNumber, @"^0+") || documentNumber.Contains('\n');
			if (result)
				return false;
			else
			{
				result = double.TryParse(documentNumber, out _);
				return result;
			}
		}

		public static string PadLeftData(string values, int cant)
		{

			string valuesFinal = "0";

			if (values != null)
			{
				valuesFinal = values.PadLeft(cant, '0');
			}

			return valuesFinal;
		}
	}
}

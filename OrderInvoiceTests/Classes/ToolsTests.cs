using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Exito.Integracion.TurboCarulla.OrderInvoice.Tests
{
	[TestClass()]
	public class ToolsTests
	{
		[TestMethod()]
		public void FindInJsonTest()
		{
			Models.OrderValidated.ResponseData requestData = new()
			{
				TraceId = System.Guid.NewGuid().ToString()
			};

			string jsonMessage = JsonConvert.SerializeObject(requestData);

			string result = Tools.FindInJson(jsonMessage, "traceId");

			Assert.AreEqual(requestData.TraceId, result);
		}

		[TestMethod()]
		public void DocumentIsValidTest()
		{
			Assert.IsTrue(Tools.DocumentIsValid("999993"));
			Assert.IsFalse(Tools.DocumentIsValid("99999333333"));
			Assert.IsFalse(Tools.DocumentIsValid("99"));
			Assert.IsFalse(Tools.DocumentIsValid("999A993"));
		}

		[TestMethod()]
		public void PadLeftDataTest()
		{
			Assert.AreEqual(Tools.PadLeftData("12", 6), "000012");
			Assert.AreEqual(Tools.PadLeftData("12", 2), "12");
		}
	}
}
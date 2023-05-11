namespace Exito.Integracion.TurboCarulla.OrderInvoice
{
	public interface IConfigData
	{
		public LocationType DefaultLocation { get; set; }
		public CustomerType DefaultCustomer { get; set; }
		public SapType SapSettings { get; set; }
		public string PluAttributes { get; set; }
	}

	public class ConfigData : IConfigData
	{
		public LocationType DefaultLocation { get; set; }
		public CustomerType DefaultCustomer { get; set; }
		public SapType SapSettings { get; set; }
		public string PluAttributes { get; set; }

	}

	public class LocationType
	{
		public int LocationId { get; set; }
		public string City { get; set; }
		public string DaneCode { get; set; }
		public string Description { get; set; }
		public string Address { get; set; }
		public string Phone { get; set; }
	}

	public class CustomerType
	{
		public double DocumentNumber { get; set; }
		public int DocumentType { get; set; }
		public string Name { get; set; }
	}

	public class SapType
	{
		public int ProcessId { get; set; }
		public string ProductSign { get; set; }
		public int OrderPrefix { get; set; }
		public string SincoId { get; set; }
		public string TypeBus { get; set; }
		public string Business { get; set; }
		public string Burks { get; set; }
		public int Vstel { get; set; }
		public string Country { get; set; }
		public string Waers { get; set; }
		public int Freight { get; set; }
		public int Insurance { get; set; }
		public int OtherCost { get; set; }
		public int Index { get; set; }
		public int LegalPersonType { get; set; }
		public string InvoiceStatus { get; set; }
		public int PaymentMethod { get; set; }
		public int Dues { get; set; }
		public int OrderRegisterId { get; set; }
		public int SellerId { get; set; }
		public int MarketPlaceOrderMovementID { get; set; }
		public int PreviousDocumentNumber { get; set; }
		public string DebtorNumber { get; set; }
		public string Bond { get; set; }
		public string ReferenceBonus { get; set; }
		public string BusinessSubType { get; set; }
		public int CollectType { get; set; }
		public string Origin { get; set; }
	}
}

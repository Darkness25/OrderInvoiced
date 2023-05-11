using Exito.Integracion.TurboCarulla.OrderInvoice.Classes;
using Exito.Integracion.TurboCarulla.OrderInvoice.Classes.Settings;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;

namespace Exito.Integracion.TurboCarulla.OrderInvoice
{

    public interface IOrderInvoice
    {
        Task<bool> SendToService(Models.OrderValidated.ResponseData responseData);
    }

    public class OrderInvoice : IOrderInvoice
    {
        private readonly IConfigData configData;
        private readonly IQueueAdapter orderValidatedQueue;
        private readonly IQueueAdapter orderDiscartQueue;
        private readonly MongoAdapter mongoAdapter;
        private readonly IRedisAdapter redisAdapter;
        private readonly ILogger logger;
        private readonly Timer aTimer;

        public OrderInvoice(IConfigData configData, QueueSettings queueSettings, IDatabaseSettings databaseSettings, IMongoClient mongoClient, IConnectionMultiplexer multiplexer, IRedisSettings redisSettings, ILogger logger)
        {
            this.configData = configData;
            this.orderValidatedQueue = new QueueAdapter(queueSettings, queueSettings.Queues[1], logger);
            this.orderDiscartQueue = new QueueAdapter(queueSettings, queueSettings.Queues[2], logger);
            this.mongoAdapter = new MongoAdapter(databaseSettings, mongoClient, logger);
            this.redisAdapter = new RedisAdapter(multiplexer, redisSettings, logger);
            this.logger = logger;

            this.orderValidatedQueue.OpenConnection();
            this.orderDiscartQueue.OpenConnection();
            aTimer = new(30000);
            aTimer.Elapsed += OnTimedEvent; aTimer.AutoReset = true; aTimer.Enabled = true;
        }

        public async Task<bool> SendToService(Models.OrderValidated.ResponseData responseData)
        {
            bool response;
            try
            {
                Models.Sap.InvoicePending.RequestData orderPendingRequest = new();
                Models.Sap.InvoiceCollect.RequestData orderCollectRequest = new();
                Models.DiscartType discartData = new()
                {
                    LocationID = responseData.OrderData.InfoGeneral.IdDependencia.ToString(),
                    OrderID = responseData.OrderData.InfoGeneral.IdPedido,
                    DateCreated = DateTime.Now,
                    IsProcessed = false,
                    IsValid = true,
                    TraceId = responseData.TraceId
                };
                Models.Pim.Maestras.ResponseData locationsData = await mongoAdapter.GetLocationDataAsync(new Models.Pim.Maestras.RequestData
                {
                    LocationId = responseData.OrderData.InfoGeneral.IdDependencia,
                    TraceId = responseData.TraceId
                });

                if (locationsData.DaneCode == null) locationsData = new Models.Pim.Maestras.ResponseData
                {
                    LocationId = configData.DefaultLocation.LocationId.ToString(),
                    DaneCode = configData.DefaultLocation.DaneCode,
                    Description = configData.DefaultLocation.Description,
                    City = configData.DefaultLocation.City,
                    Address = configData.DefaultLocation.Address,
                    Phone = configData.DefaultLocation.Phone,
                    TraceId = responseData.TraceId
                };

                Models.Sap.Poblaciones.ResponseData poblacioneData = await mongoAdapter.GetPoblacionesDataAsync(new Models.Sap.Poblaciones.RequestData
                {
                    DaneCode = int.Parse(locationsData.DaneCode),
                    TraceId = responseData.TraceId,
                });

                if (poblacioneData.Region != 0)
                {
                    orderPendingRequest = new()
                    {
                        Id = ObjectId.Parse(Tools.PadLeftData(responseData.OrderData.InfoGeneral.IdPedido, 24)),
                        OrderId = responseData.OrderData.InfoGeneral.IdPedido,
                        Origin = configData.SapSettings.Origin,
                        Status = configData.SapSettings.InvoiceStatus,
                        OrderPrefix = configData.SapSettings.OrderPrefix.ToString(),
                        OrderDate = responseData.OrderData.InfoGeneral.FechaCompra,
                        OrderTime = responseData.OrderData.InfoGeneral.HoraCompra,
                        LocationId = Tools.PadLeftData(responseData.OrderData.InfoGeneral.IdDependencia.ToString(), 2),
                        Business = configData.SapSettings.Business,
                        SincoId = configData.SapSettings.SincoId,
                        TypeBus = configData.SapSettings.TypeBus,
                        Burks = configData.SapSettings.Burks,
                        Vstel = configData.SapSettings.Vstel.ToString(),
                        Country = configData.SapSettings.Country,
                        Waers = configData.SapSettings.Waers,
                        Freight = configData.SapSettings.Freight,
                        Insurance = configData.SapSettings.Insurance,
                        OthersCost = configData.SapSettings.OtherCost,
                        Index = configData.SapSettings.Index,
                        PersonType = int.Parse(responseData.OrderData.InfoCliente.TipoDocumento.Trim()) != configData.SapSettings.LegalPersonType ? "X" : string.Empty,
                        InvoiceRetry = 1,
                        TraceId = responseData.TraceId
                    };
                    orderCollectRequest = new Models.Sap.InvoiceCollect.RequestData
                    {
                        OrderRegisterId = configData.SapSettings.OrderRegisterId,
                        OrderId = 0,
                        OrderNumber = responseData.OrderData.InfoGeneral.IdPedido,
                        CountryCode = configData.SapSettings.Country,
                        CurrencyCode = configData.SapSettings.Waers,
                        FinancialSociety = configData.SapSettings.Burks,
                        SellerId = configData.SapSettings.SellerId,
                        ReferenceBonus = configData.SapSettings.ReferenceBonus,
                        DebtorNumber = configData.SapSettings.DebtorNumber,
                        ExchangRate = 0,
                        PreviousDocumentNumber = configData.SapSettings.PreviousDocumentNumber,
                        MarketPlaceOrderMovementID = configData.SapSettings.MarketPlaceOrderMovementID,
                        BusinessSubType = configData.SapSettings.BusinessSubType,
                        Business = configData.SapSettings.Business,
                        ProcessTransactionId = responseData.TraceId,
                        CollectType = new List<int> { configData.SapSettings.CollectType }
                    };

                    if (responseData.OrderData.InfoCliente != null)
                    {
                        Models.Sap.InvoicePending.CustomerType customer = new()
                        {
                            DocumentNumber = Tools.DocumentIsValid(responseData.OrderData.InfoCliente.NroDocumento) ? responseData.OrderData.InfoCliente.NroDocumento.Trim() : configData.DefaultCustomer.DocumentNumber.ToString(),
                            DocumentType = responseData.OrderData.InfoCliente.TipoDocumento.Equals("0") ? configData.DefaultCustomer.DocumentType : int.Parse(responseData.OrderData.InfoCliente.TipoDocumento.Trim()),
                            Name = Tools.DocumentIsValid(responseData.OrderData.InfoCliente.NroDocumento) ? responseData.OrderData.InfoCliente.Nombre : configData.DefaultCustomer.Name,
                            Email = responseData.OrderData.InfoCliente.Email.Trim(),
                            PhoneNumber = string.IsNullOrEmpty(responseData.OrderData.InfoCliente.Telefono.Trim()) ? locationsData.Phone : responseData.OrderData.InfoCliente.Telefono.Trim(),
                            Address = string.IsNullOrEmpty(responseData.OrderData.InfoCliente.Direccion.Trim()) ? locationsData.Address : responseData.OrderData.InfoCliente.Direccion.Trim(),
                            City = string.IsNullOrEmpty(responseData.OrderData.InfoCliente.Ciudad.Trim()) ? poblacioneData.City.ToUpper() : responseData.OrderData.InfoCliente.Ciudad.Trim().ToUpper(),
                            Region = poblacioneData.Region
                        };
                        orderCollectRequest.CustomerID = customer.DocumentNumber;
                        orderCollectRequest.CustomerCity = customer.City;
                        orderCollectRequest.CustomerName = customer.Name;
                        orderCollectRequest.CustomerAddress = customer.Address;
                        orderCollectRequest.CustomerPhone = customer.PhoneNumber;
                        orderCollectRequest.Email = customer.Email;
                        orderCollectRequest.Region = Tools.PadLeftData(customer.Region.ToString(), 2);

                        if (string.IsNullOrEmpty(customer.Name.Trim())) customer.Name = configData.DefaultCustomer.Name;
                        orderPendingRequest.Customer = customer;
                    }
                    else
                    {
                        discartData.TypeID = "EE01";
                        discartData.Reason = "Información del cliente está incompleta";
                        discartData.IsValid = false;
                    }

                    if (responseData.OrderData.InfoGeneral != null && responseData.OrderData.InfoMedioPago != null && responseData.OrderData.InfoCliente != null)
                    {
                        Models.Sap.InvoicePending.TransactionType transaction = new()
                        {
                            Id = responseData.OrderData.InfoGeneral.IdPedido.ToString().Trim(),
                            Auth = responseData.OrderData.InfoGeneral.IdPedido.ToString().Trim(),
                            Bin = int.Parse(responseData.OrderData.InfoMedioPago.BinNumero.Trim()),
                            PaymentMethod = configData.SapSettings.PaymentMethod,
                            Dues = configData.SapSettings.Dues,
                            Value = responseData.OrderData.InfoMedioPago.ValorPago,
                            Points = Convert.ToInt32(responseData.OrderData.InfoCliente.CantidadPuntos)
                        };
                        orderPendingRequest.Transaccion = transaction;

                        Models.Sap.InvoiceCollect.PaymetType itemCollection = new()
                        {
                            TransactionId = transaction.Id,
                            AuthorizationNumber = transaction.Auth,
                            Bond = configData.SapSettings.Bond,
                            PaymentTypeCode = transaction.PaymentMethod.ToString(),
                            Amount = transaction.Value,
                            PointsRedeemed = transaction.Points,
                            Nit = (orderPendingRequest.Customer.DocumentType == configData.SapSettings.LegalPersonType) ? orderPendingRequest.Customer.DocumentNumber : ""
                        };

                        orderCollectRequest.CollectItems = new List<Models.Sap.InvoiceCollect.PaymetType> { itemCollection };
                    }
                    else
                    {
                        discartData.TypeID = "EE01";
                        discartData.Reason = "Información del transacción y medio de pago está incompleta";
                        discartData.IsValid = false;
                    }

                    if (responseData.OrderData.InfoProducto != null)
                    {
                        List<Models.Sap.InvoicePending.ProductType> products = new();
                        List<Models.Sap.InvoiceCollect.PluType> pluList = new();

                        foreach (Models.OrderValidated.InfoProductoType product in responseData.OrderData.InfoProducto)
                        {
                            Models.Sap.InvoicePending.ProductType newProduct = new()
                            {
                                Plu = product.Producto.Plu,
                                Description = product.Producto.Descripcion,
                                Discout = product.Producto.ValorDescuento,
                                IvaRate = product.Producto.Iva,
                                MaterialCode = product.Producto.MaterialCode,
                                PosNumber = product.Producto.PosNumero,
                                Price = product.Producto.ValorVenta,
                                Quantity = (float)product.Producto.Cantidad,
                                Sequence = product.Producto.Secuencia,
                                Sign = product.Producto.Signo,
                                SubLine = product.Producto.Sublinea
                            };
                            Models.Sap.InvoiceCollect.PluType newPlu = new()
                            {
                                OrderLineId = product.Producto.PosNumero.ToString(),
                                Plu = product.Producto.Plu
                            };
                            products.Add(newProduct);
                            pluList.Add(newPlu);
                        }

                        orderPendingRequest.Products = products.ToArray();
                        orderCollectRequest.Plus = pluList;
                        orderCollectRequest.TraceId = responseData.TraceId;
                    }
                    else
                    {
                        discartData.TypeID = "EE01";
                        discartData.Reason = "Información del productos está incompleta";
                        discartData.IsValid = false;
                    }
                }
                else
                {
                    discartData.TypeID = "EE04";
                    discartData.Reason = "La región es: " + poblacioneData.Region;
                    discartData.IsValid = false;
                }

                if (discartData.IsValid)
                {
                    List<Models.Sap.InvoiceCollect.RequestData> orderCollectRequestList = new() { orderCollectRequest };
                    Models.Sap.InvoicePending.ResponseData pendingResponse = await mongoAdapter.InsertInvoiceAsyc(orderPendingRequest);
                    if (!pendingResponse.IsDiscarted)
                    {
                        if (await QueueMessage(orderValidatedQueue, JsonConvert.SerializeObject(orderCollectRequestList.ToArray())))
                        {
                            logger.LogInformation("Mensaje procesado {discartData.TraceId}", discartData.TraceId);
                            await redisAdapter.UpdateCacheAsync(responseData);
                            response = true;
                        }
                        else
                        {
                            await mongoAdapter.DeleteInvoiceAsync(orderPendingRequest);
                            response = false;
                        }
                    }
                    else
                    {
                        discartData.TypeID = "EE10";
                        discartData.IsValid = false;
                        discartData.Reason = "No se pudo insertar en ordenes: Orden existente";
                        await QueueMessage(orderDiscartQueue, JsonConvert.SerializeObject(discartData));
                        response = true;
                    }                    
                }
                else
                    response = await QueueMessage(orderDiscartQueue, JsonConvert.SerializeObject(discartData));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[OrderInvoice] Failed connect to services", responseData);
                await DataTracker.TrackEventAsync(null, responseData, "OrderInvoice/Dequeue/response: " + ex.GetHashCode().ToString(), responseData.TraceId, ex);
                response = false;
            }
            return response;
        }

        private async Task<bool> QueueMessage(IQueueAdapter queueAdapter, string queueMessage)
        {
            string traceId = Tools.FindInJson(queueMessage, "traceId");
            await DataTracker.TrackEventAsync(new object(), queueMessage, "OrderInvoice/Queue/request: QueueName=" + queueAdapter.GetQueue().QueueName, traceId, null);
            bool response;

            try
            {
                if (queueAdapter.GetChannel().IsClosed) queueAdapter.OpenConnection();
                if (queueAdapter.GetChannel().IsOpen)
                {
                    response = await queueAdapter.QueueMessageAsync(queueMessage);
                    await DataTracker.TrackEventAsync(null, response, "OrderInvoice/Queue/response: " + response, traceId, null);
                }
                else throw new ApplicationException("Queue connection failed");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[OrderInvoice] Cannot enqueue message: {ex.Message}", ex.Message);
                await DataTracker.TrackEventAsync(null, ex.Message, "OrderInvoice/Queue/response: " + ex.Source, traceId, ex);
                response = false;
            }

            return response;
        }

        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            if (orderValidatedQueue.GetChannel().IsOpen) orderValidatedQueue.CloseConnection();
            if (orderDiscartQueue.GetChannel().IsOpen) orderDiscartQueue.CloseConnection();
        }
    }
}

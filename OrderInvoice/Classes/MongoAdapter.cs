using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Exito.Integracion.TurboCarulla.OrderInvoice
{
    public class MongoAdapter
    {
        private readonly IMongoCollection<Models.Pim.Maestras.ResponseData> LocationData;
        private readonly IMongoCollection<Models.Sap.InvoicePending.RequestData> InvoiceData;
        private readonly IMongoCollection<Models.Sap.Poblaciones.ResponseData> PoblacionesData;
        private readonly ILogger logger;

        public MongoAdapter(IDatabaseSettings dbSettings, IMongoClient client, ILogger logger)
        {
            try
            {
                IMongoDatabase mongoDb = client.GetDatabase(dbSettings.Mongo.Databases[0].DatabaseName);
                LocationData = mongoDb.GetCollection<Models.Pim.Maestras.ResponseData>(dbSettings.Mongo.Databases[0].CollectionName);

                mongoDb = client.GetDatabase(dbSettings.Mongo.Databases[1].DatabaseName);
                InvoiceData = mongoDb.GetCollection<Models.Sap.InvoicePending.RequestData>(dbSettings.Mongo.Databases[1].CollectionName);

                mongoDb = client.GetDatabase(dbSettings.Mongo.Databases[2].DatabaseName);
                PoblacionesData = mongoDb.GetCollection<Models.Sap.Poblaciones.ResponseData>(dbSettings.Mongo.Databases[2].CollectionName);
            }
            catch (Exception ex)
            {
                logger.LogError("[OrderInvoice] Error: {GetType().Name} - {ex.Message}", GetType().Name, ex.Message);
            }
            this.logger = logger;
        }

        public async Task<Models.Pim.Maestras.ResponseData> GetLocationDataAsync(Models.Pim.Maestras.RequestData requestData)
        {
            Models.Pim.Maestras.ResponseData responseData = new() { TraceId = requestData.TraceId };
            try
            {
                string locationId = requestData.LocationId.ToString().PadLeft(4, '0');
                await DataTracker.TrackEventAsync(new object(), requestData, "OrderInvoice/NovedadesDependencias/request:", requestData.TraceId, null);
                responseData = LocationData.Find(c => c.LocationId.Equals(locationId)).FirstOrDefault();
                if (responseData != null) responseData.TraceId = requestData.TraceId;
                else responseData = new() { TraceId = requestData.TraceId };
                await DataTracker.TrackEventAsync(new object(), responseData, "OrderInvoice/NovedesDependencias/response:", responseData.TraceId, null);
            }
            catch (Exception ex)
            {
                await DataTracker.TrackEventAsync(new object(), responseData, "OrderInvoice/NovedesDependencias/response:" + ex.GetHashCode(), requestData.TraceId, ex);
                logger.LogError("[OrderInvoice] Error: {GetType().Name} - {ex.Message}", GetType().Name, ex.Message);
            }

            return string.IsNullOrEmpty(responseData.DaneCode) ? null : responseData;
        }

        public async Task<Models.Sap.Poblaciones.ResponseData> GetPoblacionesDataAsync(Models.Sap.Poblaciones.RequestData requestData)
        {
            Models.Sap.Poblaciones.ResponseData responseData = new() { TraceId = requestData.TraceId };
            try
            {
                await DataTracker.TrackEventAsync(new object(), requestData, "OrderInvoice/PoblacionesSap/request:", requestData.TraceId, null);
                responseData = PoblacionesData.Find(c => c.DaneCode == requestData.DaneCode).FirstOrDefault();
                if (responseData != null) responseData.TraceId = requestData.TraceId;
                else responseData = new() { TraceId = requestData.TraceId };
                await DataTracker.TrackEventAsync(new object(), responseData, "OrderInvoice/PoblacionesSap/response:", responseData.TraceId, null);
            }
            catch (Exception ex)
            {
                await DataTracker.TrackEventAsync(new object(), responseData, "OrderInvoice/PoblacionesSap/response:" + ex.GetHashCode(), requestData.TraceId, ex);
                logger.LogError("[OrderInvoice] Error: {GetType().Name} - {ex.Message}", GetType().Name, ex.Message);
            }

            return string.IsNullOrEmpty(responseData.City) ? null : responseData;
        }

        public async Task<Models.Sap.InvoicePending.ResponseData> InsertInvoiceAsyc(Models.Sap.InvoicePending.RequestData requestData)
        {
            Models.Sap.InvoicePending.ResponseData responseData = new() { TraceId = requestData.TraceId };
            int tryInsert = 0;
            bool validInsert = false;
            CancellationTokenSource cancelacionInsert = new CancellationTokenSource(TimeSpan.FromSeconds(1));            
            while (tryInsert < 3 && !validInsert)
            {
                tryInsert++;
                try
                {
                    await DataTracker.TrackEventAsync(new object(), requestData, "OrderInvoice/PendingInvoice/request:", requestData.TraceId, null);
                    var task = InvoiceData.InsertOneAsync(requestData);
                    var completado = await Task.WhenAny(task, Task.Delay(-1, cancelacionInsert.Token)); // Esperar la tarea de inserción o el tiempo límite
                    if (completado == task) // Si se completó la tarea de inserción
                    {
                        task.Wait(); // Esperar a que termine la tarea                        
                        validInsert = true;
                        responseData.IsDiscarted = false;
                        await DataTracker.TrackEventAsync(new object(), responseData, "OrderInvoice/PendingInvoice/response:", responseData.TraceId, null);
                    }
                }

                catch (MongoWriteException ex)
                {
                    await DataTracker.TrackEventAsync(new object(), responseData, "OrderInvoice/PendingInvoice/response:", responseData.TraceId, null);
                    logger.LogError("[OrderInvoice] Orden descartada: {requestData.OrderId} - ya existe", requestData.OrderId);
                    responseData = new Models.Sap.InvoicePending.ResponseData
                    {
                        IsDiscarted = true,
                        Message = ex.Message,
                        TraceId = requestData.TraceId
                    };
                }
                catch (Exception ex)
                {
                    await DataTracker.TrackEventAsync(new object(), responseData, "OrderInvoice/PendingInvoice/response:" + ex.GetHashCode(), requestData.TraceId, ex);
                    logger.LogError("[OrderInvoice] Error: {GetType().Name} - {ex.Message}", GetType().Name, ex.Message);
                    responseData.IsDiscarted = true;
                    responseData.Message = ex.Message;
                }
            }

            return responseData;
        }

        public async Task<Models.Sap.InvoicePending.ResponseData> DeleteInvoiceAsync(Models.Sap.InvoicePending.RequestData requestData)
        {
            Models.Sap.InvoicePending.ResponseData responseData = new() { TraceId = requestData.TraceId };
            int maxRetryAttempts = 3; // Máximo de 3 intentos
            int retryAttempt = 0;
            bool retry;

            do
            {
                retry = false;
                try
                {
                    await DataTracker.TrackEventAsync(new object(), requestData, "OrderInvoice/PendingInvoice/request:", requestData.TraceId, null);
                    var filter = Builders<Models.Sap.InvoicePending.RequestData>.Filter.Eq(r => r.Id, requestData.Id);
                    var result = await InvoiceData.DeleteOneAsync(filter);

                    if (result.DeletedCount == 1)
                    {
                        return new Models.Sap.InvoicePending.ResponseData { IsDiscarted = false };
                    }
                    else
                    {
                        return new Models.Sap.InvoicePending.ResponseData { IsDiscarted = true, };
                    }
                }
                catch (Exception ex) when (retryAttempt < maxRetryAttempts)
                {
                    retryAttempt++;
                    retry = true;
                    await DataTracker.TrackEventAsync(new object(), responseData, "OrderInvoice/PendingInvoice/response:" + ex.GetHashCode(), requestData.TraceId, ex);
                    logger.LogWarning("[OrderInvoice] Warning: {GetType().Name} - {ex.Message}. Retrying in 1 seconds...", GetType().Name, ex.Message);
                    await Task.Delay(1000); // Espera 5 segundos antes de volver a intentarlo
                }
                catch (Exception ex)
                {
                    await DataTracker.TrackEventAsync(new object(), responseData, "OrderInvoice/PendingInvoice/response:" + ex.GetHashCode(), requestData.TraceId, ex);
                    logger.LogError("[OrderInvoice] Error: {GetType().Name} - {ex.Message}", GetType().Name, ex.Message);
                    responseData.IsDiscarted = false;
                    responseData.Message = ex.Message;
                }
            } while (retry);

            return responseData;
        }
    }
}
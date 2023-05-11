using Exito.Integracion.TurboCarulla.OrderInvoice.Classes.Settings;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace Exito.Integracion.TurboCarulla.OrderInvoice.Classes
{
	public interface IRedisAdapter
	{
		Task<bool> UpdateCacheAsync(Models.OrderValidated.ResponseData responseData);
	}

	public class RedisAdapter : IRedisAdapter
	{
		private readonly IConnectionMultiplexer multiplexer;
		private readonly IRedisSettings redisSettings;
		private readonly ILogger logger;

		public RedisAdapter(IConnectionMultiplexer multiplexer, IRedisSettings redisSettings, ILogger logger)
		{
			this.multiplexer = multiplexer;
			this.redisSettings = redisSettings;
			this.logger = logger;
		}

		public async Task<bool> UpdateCacheAsync(Models.OrderValidated.ResponseData responseData)
		{
			try
			{
				TimeSpan defaultExpiry = TimeSpan.FromHours(redisSettings.DefaultExpiry);
				IDatabase redisdb = multiplexer.GetDatabase();
				await DataTracker.TrackEventAsync(defaultExpiry, responseData, "OrderInvoice/RedisUpdate/request:" + responseData.OrderData.InfoGeneral.IdOrderPos, responseData.TraceId, null);
				return await redisdb.StringSetAsync($"orderNumber:{responseData.OrderData.InfoGeneral.IdOrderPos}", responseData.OrderData.InfoGeneral.IdOrderPos, defaultExpiry);
			}
			catch (Exception ex)
			{
				await DataTracker.TrackEventAsync(multiplexer, ex.Message, "UsualProducts/RedisUpdate/response:" + ex.GetHashCode(), responseData.TraceId, ex);
				logger.LogError(ex, "Cannot connect to redis services: {ex.Message}", ex.Message);
				return false;
			}
		}
	}
}

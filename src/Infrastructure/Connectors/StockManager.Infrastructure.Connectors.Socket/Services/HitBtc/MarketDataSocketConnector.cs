﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StockManager.Domain.Core.Enums;
using StockManager.Infrastructure.Connectors.Common.Models.Market;
using StockManager.Infrastructure.Connectors.Common.Services;
using StockManager.Infrastructure.Connectors.Socket.Connection;
using StockManager.Infrastructure.Connectors.Socket.Models.NotificationParameters;
using StockManager.Infrastructure.Connectors.Socket.Models.RequestParameters;
using StockManager.Infrastructure.Connectors.Socket.Models.Market;

namespace StockManager.Infrastructure.Connectors.Socket.Services.HitBtc
{
	public class MarketDataSocketConnector : IMarketDataSocketConnector
	{
		private readonly HitBtcConnection _connection;

		public MarketDataSocketConnector()
		{
			_connection = new HitBtcConnection();
		}

		public async Task<IList<Infrastructure.Common.Models.Market.CurrencyPair>> GetCurrencyPairs()
		{
			var request = new SingleSocketRequest<EmptyRequestParameters>
			{
				RequestMethodName = "getSymbols"
			};
			var currencyPairsInner = await _connection.DoRequest<CurrencyPair[]>(request);
			var currencyPairs = currencyPairsInner
				.Select(entity => entity.ToOuterModel())
				.ToList();

			return currencyPairs;
		}

		public async Task<Infrastructure.Common.Models.Market.CurrencyPair> GetCurrencyPair(string id)
		{
			var request = new SingleSocketRequest<CurrencyRequestParameters>
			{
				RequestMethodName = "getSymbol",
				RequestParameters = new CurrencyRequestParameters
				{
					CurrencyId = id
				}
			};
			var currencyPairInner = await _connection.DoRequest<CurrencyPair>(request);
			var currencyPair = currencyPairInner.ToOuterModel();

			return currencyPair;
		}

		public async Task SubscribeOnCandles(string currencyPairId, CandlePeriod period, Action<IList<Infrastructure.Common.Models.Market.Candle>> callback, int limit = 30)
		{
			var request = new SocketSubscriptionRequest<CandleRequestParameters>
			{
				RequestMethodName = "subscribeCandles",
				NotificationMethodNames =
				{
					"snapshotCandles",
					"updateCandles"
				},
				RequestParameters = new CandleRequestParameters
				{
					CurrencyPairId = currencyPairId,
					Period = period.ToInnerFormat(),
					Limit = limit
				}
			};

			await _connection.Subscribe<CandleNotificationParameters>(request, notificationParameters =>
			{
				var notificationCurrencyPairId = notificationParameters.CurrencyPairId;
				var notificationPeriod = CandlePeriodMap.ToOuterFormat(notificationParameters.Period);

				if (!(currencyPairId == notificationCurrencyPairId && notificationPeriod == period))
					return;

				callback(notificationParameters.Candles
					.Select(CandleMap.ToOuterModel)
					.ToList());
			});
		}

		public void SubscribeOnTickers(Action<IList<Infrastructure.Common.Models.Market.Ticker>> callback)
		{
			throw new NotImplementedException();
		}
	}
}
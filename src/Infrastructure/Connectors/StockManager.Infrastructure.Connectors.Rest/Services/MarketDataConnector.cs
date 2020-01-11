﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RestSharp;
using StockManager.Domain.Core.Enums;
using StockManager.Infrastructure.Connectors.Common.Common;
using StockManager.Infrastructure.Connectors.Common.Services;
using StockManager.Infrastructure.Connectors.Rest.Connection;
using StockManager.Infrastructure.Connectors.Rest.Models.Market;

namespace StockManager.Infrastructure.Connectors.Rest.Services
{
	public class MarketDataConnector : IMarketDataConnector
	{
		public async Task<IList<Infrastructure.Common.Models.Market.CurrencyPair>> GetCurrensyPairs()
		{
			var connection = new ApiConnection();
			var request = new RestRequest("public/symbol", Method.GET);
			request.Configure();
			var response = await connection.DoRequest(request);
			var currencyPairs = response
				.ExtractData<CurrencyPair[]>()
				.Select(entity => entity.ToOuterModel())
				.ToList();
			return currencyPairs;
		}

		public async Task<Infrastructure.Common.Models.Market.CurrencyPair> GetCurrensyPair(string id)
		{
			var connection = new ApiConnection();
			var request = new RestRequest(String.Format("public/symbol/{0}", id), Method.GET);
			request.Configure();
			var response = await connection.DoRequest(request);
			var currencyPair = response
				.ExtractData<CurrencyPair>()
				?.ToOuterModel();
			return currencyPair;
		}

		public async Task<IList<Infrastructure.Common.Models.Market.Candle>> GetCandles(String currencyPairId, CandlePeriod period, int limit)
		{
			var connection = new ApiConnection();
			var request = new RestRequest(String.Format("public/candles/{0}", currencyPairId), Method.GET);
			request.Configure();

			string candlePeriod;
			switch (period)
			{
				case CandlePeriod.Minute1:
					candlePeriod = "M1";
					break;
				case CandlePeriod.Minute3:
					candlePeriod = "M3";
					break;
				case CandlePeriod.Minute5:
					candlePeriod = "M5";
					break;
				case CandlePeriod.Minute15:
					candlePeriod = "M15";
					break;
				case CandlePeriod.Minute30:
					candlePeriod = "M30";
					break;
				case CandlePeriod.Hour1:
					candlePeriod = "H1";
					break;
				case CandlePeriod.Hour4:
					candlePeriod = "H4";
					break;
				case CandlePeriod.Day1:
					candlePeriod = "D1";
					break;
				case CandlePeriod.Day7:
					candlePeriod = "D7";
					break;
				case CandlePeriod.Month1:
					candlePeriod = "1M";
					break;
				default:
					throw new ConnectorException("Undefined candle period", null);
			}
			request.AddParameter("period", candlePeriod);

			request.AddParameter("limit", limit);

			var response = await connection.DoRequest(request);
			var candles = response
				.ExtractData<Candle[]>()
				.Select(entity => entity.ToOuterModel())
				.ToList();

			return candles;
		}

		public async Task<IList<Infrastructure.Common.Models.Market.OrderBookItem>> GetOrderBook(string currencyPairId, int limit)
		{
			var connection = new ApiConnection();
			var request = new RestRequest(String.Format("public/orderbook/{0}", currencyPairId), Method.GET);
			request.Configure();

			request.AddParameter("limit", limit);

			var response = await connection.DoRequest(request);
			var orderBookItems = response
				.ExtractData<OrderBook>()
				?.ToOuterModel();

			return orderBookItems ?? new Infrastructure.Common.Models.Market.OrderBookItem[] { };
		}

		public async Task<IList<Infrastructure.Common.Models.Market.Ticker>> GetTickers()
		{
			var connection = new ApiConnection();
			var request = new RestRequest("public/ticker", Method.GET);
			request.Configure();
			var response = await connection.DoRequest(request);
			var tickers = response
				.ExtractData<Ticker[]>()
				.Select(entity => entity.ToOuterModel())
				.ToList();
			return tickers;
		}

		public async Task<Infrastructure.Common.Models.Market.Ticker> GetTicker(string currencyPairId)
		{
			var connection = new ApiConnection();
			var request = new RestRequest(String.Format("public/ticker/{0}", currencyPairId), Method.GET);
			request.Configure();
			var response = await connection.DoRequest(request);
			var ticker = response
				.ExtractData<Ticker>()
				?.ToOuterModel();
			return ticker;
		}
	}
}
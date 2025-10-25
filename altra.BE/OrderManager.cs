using KiteConnect;
using Microsoft.AspNetCore.Mvc;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Numerics;
using System.Text.Json;
using System.Transactions;
using System.Windows;
using System.Windows.Markup;
using static System.Net.WebRequestMethods;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace altra.BE
{
    public class OrderManager
    {
        public List<KiteConnect.Instrument> AvailableScrips = new List<KiteConnect.Instrument>();
        public Kite _kite;
        private Ticker ticker;
        public EventHandler ServerConnected;
        public EventHandler ServerDisconnected;
        public Action<int, string> IndexUpdated;

        //private ObservableCollection<StockItem> _selectedStocks;

        public OrderManager(Kite kite)
        {
            _kite = kite;
        }

        public string PlaceOpenOrder(string symbol, int quantity, decimal buy, decimal sell)
        {
            return buy != 0 ? PlaceGTTOrder(symbol, quantity, buy, Constants.TRANSACTION_TYPE_BUY) : PlaceGTTOrder(symbol, quantity, sell, Constants.TRANSACTION_TYPE_SELL);
        }

        public void CancelAllGTTOrders()
        {
            foreach (var gtt in _kite.GetGTTs())
            {
                var gttCancelResponse = _kite.CancelGTT(gtt.Id);
                Logger.Log(Utils.JsonSerialize(gttCancelResponse));
            }
        }

        private string PlaceGTTOrder(string symbol, int quantity, decimal price, string type)
        {
            try
            {
                var ltp = GrowwApi.GetLTP(symbol);

                GTTParams gttParams = new GTTParams
                {
                    TriggerType = Constants.GTT_TRIGGER_SINGLE,
                    Exchange = "NSE",
                    TradingSymbol = symbol,
                    LastPrice = ltp,
                    TriggerPrices = new List<decimal>() { price },
                    Orders = new List<GTTOrderParams>
                    {
                        new GTTOrderParams
                        {
                            OrderType = Constants.ORDER_TYPE_LIMIT,
                            Price = price,
                            Product = Constants.PRODUCT_CNC,
                            TransactionType = type,
                            Quantity = quantity
                        }
                    }
                };

                var placeGTTResponse = _kite.PlaceGTT(gttParams);
                var status = placeGTTResponse["status"];

                Logger.Log($"Placing reverse GTT {type} order for {symbol} with ltp: {ltp} at price {price} got {status}");
                return status;
            }
            catch (Exception e)
            {
                Logger.Log($"FAILED Placing reverse GTT {type} order for {symbol} because {e.Message}");
                return e.Message;
            }
        }

    }
}

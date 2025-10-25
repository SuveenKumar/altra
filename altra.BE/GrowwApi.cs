using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;

namespace altra.BE
{

    public class GrowwApi
    {
        public class StockData
        {
            public decimal ltp { get; set; }
        }

        public static decimal GetLTP(string symbol)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    string url = $"https://groww.in/v1/api/stocks_data/v1/accord_points/exchange/NSE/segment/CASH/latest_prices_ohlc/{symbol}";
                    var response = client.GetAsync(url).Result;
                    response.EnsureSuccessStatusCode();

                    var jsonString = response.Content.ReadAsStringAsync().Result;
                    var stockData = JsonSerializer.Deserialize<StockData>(jsonString);

                    return stockData.ltp;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching LTP: {ex.Message}");
                return -1;
            }
        }
    }
}
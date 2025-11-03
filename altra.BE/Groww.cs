
using System.Text.Json;

namespace altra.BE
{
    public class StockData
    {
        public decimal ltp { get; set; }
    }
    public class Groww
    {
        private readonly HttpClient _httpClient;

        public Groww(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<decimal> GetLatestPricesAsync(string symbol)
        {
            try
            {
                var url = $"https://groww.in/v1/api/stocks_data/v1/accord_points/exchange/NSE/segment/CASH/latest_prices_ohlc/{symbol}";

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("User-Agent",
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/117.0.0.0 Safari/537.36");
                request.Headers.Add("Accept", "application/json, text/plain, */*");
                request.Headers.Add("Referer", "https://groww.in/");
                request.Headers.Add("Origin", "https://groww.in");

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var jsonString = await response.Content.ReadAsStringAsync();
                var stockData = JsonSerializer.Deserialize<StockData>(jsonString);

                return stockData.ltp;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching LTP: {ex.Message}");
                return -1;
            }

        }
    }

}


using System.Text.Json;

namespace altra.BE
{
    public class GrowwPriceInfo
    {
        public decimal lastPrice { get; set; }
    }

    public class GrowwResponse
    {
        public GrowwPriceInfo priceInfo { get; set; }
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
                var url = $"https://www.nseindia.com/api/quote-equity?symbol={symbol}";

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("User-Agent",
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/117.0.0.0 Safari/537.36");
                request.Headers.Add("Accept", "application/json, text/plain, */*");
                request.Headers.Add("Referer", "https://groww.in/");
                request.Headers.Add("Origin", "https://groww.in");

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var jsonString = await response.Content.ReadAsStringAsync();
                var stockData = JsonSerializer.Deserialize<GrowwResponse>(jsonString);

                return stockData.priceInfo.lastPrice;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching LTP: {ex.Message}");
                return -1;
            }

        }
    }

}

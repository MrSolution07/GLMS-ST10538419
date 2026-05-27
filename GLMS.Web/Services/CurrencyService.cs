using System.Text.Json;

namespace GLMS.Web.Services;

/// <summary>
/// Fetches the live USD→ZAR exchange rate from ExchangeRate-API using async HttpClient.
/// Falls back to a sensible default if the external API is unavailable.
/// </summary>
public class CurrencyService : ICurrencyService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CurrencyService> _logger;

    private const decimal FallbackRate = 18.50m;

    public CurrencyService(HttpClient httpClient, IConfiguration configuration, ILogger<CurrencyService> logger)
    {
        _httpClient    = httpClient;
        _configuration = configuration;
        _logger        = logger;
    }

    public async Task<decimal> GetUsdToZarRateAsync()
    {
        try
        {
            var apiKey  = _configuration["CurrencyApi:ApiKey"] ?? "latest";
            var baseUrl = _configuration["CurrencyApi:BaseUrl"]
                          ?? "https://open.er-api.com/v6/latest/USD";

            var url = string.IsNullOrEmpty(_configuration["CurrencyApi:ApiKey"])
                      ? "https://open.er-api.com/v6/latest/USD"
                      : $"https://v6.exchangerate-api.com/v6/{apiKey}/latest/USD";

            using var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;

            if (root.TryGetProperty("rates", out var rates) &&
                rates.TryGetProperty("ZAR", out var zarElement))
            {
                return zarElement.GetDecimal();
            }

            _logger.LogWarning("ZAR rate not found in API response; using fallback {Rate}.", FallbackRate);
            return FallbackRate;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Currency API call failed; using fallback rate {Rate}.", FallbackRate);
            return FallbackRate;
        }
    }

    public decimal ConvertUsdToZar(decimal usdAmount, decimal rate)
    {
        if (rate <= 0)
            throw new ArgumentOutOfRangeException(nameof(rate), "Exchange rate must be positive.");

        if (usdAmount < 0)
            throw new ArgumentOutOfRangeException(nameof(usdAmount), "USD amount cannot be negative.");

        return Math.Round(usdAmount * rate, 2);
    }
}

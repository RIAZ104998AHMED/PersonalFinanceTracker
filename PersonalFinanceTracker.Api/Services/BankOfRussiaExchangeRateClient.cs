using System.Globalization;
using System.Text;
using System.Xml.Linq;
using Microsoft.Extensions.Options;
using PersonalFinanceTracker.BusinessLogic.Configuration;
using PersonalFinanceTracker.BusinessLogic.Interfaces;
using PersonalFinanceTracker.BusinessLogic.Models;

namespace PersonalFinanceTracker.Api.Services;

public sealed class BankOfRussiaExchangeRateClient
    : IExchangeRateClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BankOfRussiaExchangeRateClient> _logger;

    public BankOfRussiaExchangeRateClient(
        HttpClient httpClient,
        ILogger<BankOfRussiaExchangeRateClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        Encoding.RegisterProvider(
            CodePagesEncodingProvider.Instance);
    }

    public async Task<IReadOnlyCollection<ExchangeRateValue>>
        GetRatesAsync(
            DateOnly date,
            CancellationToken cancellationToken = default)
    {
        var requestPath =
            $"scripts/XML_daily.asp?date_req={date:dd/MM/yyyy}";

        var startedAt = DateTime.UtcNow;

        try
        {
            using var response =
                await _httpClient.GetAsync(
                    requestPath,
                    cancellationToken);

            var duration =
                DateTime.UtcNow - startedAt;

            _logger.LogInformation(
                "Exchange rate request for {Date} returned {StatusCode} in {DurationMs} ms",
                date,
                (int)response.StatusCode,
                duration.TotalMilliseconds);

            response.EnsureSuccessStatusCode();

            var bytes =
                await response.Content
                    .ReadAsByteArrayAsync(
                        cancellationToken);

            var encoding =
                Encoding.GetEncoding(
                    "windows-1251");

            var xml =
                encoding.GetString(bytes);

            var document =
                XDocument.Parse(xml);

            var rates =
                new List<ExchangeRateValue>();

            foreach (var element in
                     document.Descendants("Valute"))
            {
                var currency =
                    element.Element("CharCode")
                        ?.Value
                        ?.Trim();

                var nominalText =
                    element.Element("Nominal")
                        ?.Value
                        ?.Trim();

                var valueText =
                    element.Element("Value")
                        ?.Value
                        ?.Trim();

                if (string.IsNullOrWhiteSpace(currency) ||
                    string.IsNullOrWhiteSpace(nominalText) ||
                    string.IsNullOrWhiteSpace(valueText))
                {
                    continue;
                }

                if (!decimal.TryParse(
                        nominalText,
                        NumberStyles.Number,
                        CultureInfo.InvariantCulture,
                        out var nominal))
                {
                    continue;
                }

                var russianCulture =
                    CultureInfo.GetCultureInfo("ru-RU");

                if (!decimal.TryParse(
                        valueText,
                        NumberStyles.Number,
                        russianCulture,
                        out var value))
                {
                    continue;
                }

                if (nominal <= 0)
                {
                    continue;
                }

                var rate =
                    value / nominal;

                rates.Add(
                    new ExchangeRateValue(
                        currency,
                        rate));
            }

            return rates;
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "Exchange rate request failed for {Date}",
                date);

            throw;
        }
    }
}
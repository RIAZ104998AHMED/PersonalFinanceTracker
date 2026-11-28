namespace PersonalFinanceTracker.BusinessLogic.Configuration;

public sealed class ExchangeRatesOptions
{
    public const string SectionName = "ExchangeRates";

    public string BaseUrl { get; set; } = string.Empty;

    public int TimeoutSeconds { get; set; } = 10;
}
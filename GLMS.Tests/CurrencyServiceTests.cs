using GLMS.Web.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace GLMS.Tests;

/// <summary>
/// Unit tests for CurrencyService — verifies the USD→ZAR conversion mathematics
/// under a controlled, known exchange rate to ensure precision.
/// </summary>
public class CurrencyServiceTests
{
    private readonly CurrencyService _sut;

    public CurrencyServiceTests()
    {
        var httpClient    = new HttpClient();
        var config        = new Mock<IConfiguration>();
        var logger        = new Mock<ILogger<CurrencyService>>();

        config.Setup(c => c["CurrencyApi:ApiKey"]).Returns(string.Empty);

        _sut = new CurrencyService(httpClient, config.Object, logger.Object);
    }

    [Fact(DisplayName = "ConvertUsdToZar: R18.50 rate multiplies correctly")]
    public void ConvertUsdToZar_KnownRate_ReturnsCorrectAmount()
    {
        // Arrange
        decimal usdAmount    = 100m;
        decimal exchangeRate = 18.50m;

        // Act
        decimal result = _sut.ConvertUsdToZar(usdAmount, exchangeRate);

        // Assert
        Assert.Equal(1850.00m, result);
    }

    [Fact(DisplayName = "ConvertUsdToZar: Rounds to two decimal places")]
    public void ConvertUsdToZar_UnevenRate_RoundsTwoDecimals()
    {
        decimal result = _sut.ConvertUsdToZar(1m, 18.3333m);

        Assert.Equal(18.33m, result);
    }

    [Fact(DisplayName = "ConvertUsdToZar: Zero USD returns zero ZAR")]
    public void ConvertUsdToZar_ZeroAmount_ReturnsZero()
    {
        decimal result = _sut.ConvertUsdToZar(0m, 18.50m);

        Assert.Equal(0m, result);
    }

    [Fact(DisplayName = "ConvertUsdToZar: Large amount converts precisely")]
    public void ConvertUsdToZar_LargeAmount_IsAccurate()
    {
        decimal result = _sut.ConvertUsdToZar(50_000m, 18.50m);

        Assert.Equal(925_000.00m, result);
    }

    [Fact(DisplayName = "ConvertUsdToZar: Negative USD throws ArgumentOutOfRangeException")]
    public void ConvertUsdToZar_NegativeUsd_ThrowsException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _sut.ConvertUsdToZar(-10m, 18.50m));
    }

    [Fact(DisplayName = "ConvertUsdToZar: Zero or negative rate throws ArgumentOutOfRangeException")]
    public void ConvertUsdToZar_ZeroRate_ThrowsException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _sut.ConvertUsdToZar(100m, 0m));
    }

    [Fact(DisplayName = "ConvertUsdToZar: Negative rate throws ArgumentOutOfRangeException")]
    public void ConvertUsdToZar_NegativeRate_ThrowsException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _sut.ConvertUsdToZar(100m, -1m));
    }

    [Theory(DisplayName = "ConvertUsdToZar: Various amounts produce exact results")]
    [InlineData(1,       18.50, 18.50)]
    [InlineData(10,      18.50, 185.00)]
    [InlineData(100,     18.50, 1850.00)]
    [InlineData(1000,    18.50, 18500.00)]
    [InlineData(0.50,    18.50, 9.25)]
    public void ConvertUsdToZar_Theory_VariousAmounts(
        double usd, double rate, double expected)
    {
        var result = _sut.ConvertUsdToZar((decimal)usd, (decimal)rate);
        Assert.Equal((decimal)expected, result);
    }
}

using Moq;
using MiraiShop.Application.Services;
using MiraiShop.Domain.Entities;
using MiraiShop.Domain.Interfaces;

namespace MiraiShop.Tests;

public class WeatherForecastServiceTests
{
    private readonly Mock<IWeatherForecastRepository> _repositoryMock;
    private readonly WeatherForecastService _service;

    public WeatherForecastServiceTests()
    {
        _repositoryMock = new Mock<IWeatherForecastRepository>();
        _service = new WeatherForecastService(_repositoryMock.Object);
    }

    [Fact]
    public void GetForecasts_ReturnsMappedDtos()
    {
        var fakeData = new List<WeatherForecast>
        {
            new() { Date = new DateOnly(2026, 5, 27), TemperatureC = 20, Summary = "Warm" },
            new() { Date = new DateOnly(2026, 5, 28), TemperatureC = -5, Summary = "Freezing" }
        };
        _repositoryMock.Setup(r => r.GetForecasts(2)).Returns(fakeData);

        var result = _service.GetForecasts(2).ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal(new DateOnly(2026, 5, 27), result[0].Date);
        Assert.Equal(20, result[0].TemperatureC);
        Assert.Equal("Warm", result[0].Summary);
        Assert.Equal(-5, result[1].TemperatureC);
        Assert.Equal("Freezing", result[1].Summary);
    }

    [Fact]
    public void GetForecasts_TemperatureF_IsConvertedCorrectly()
    {
        var fakeData = new List<WeatherForecast>
        {
            new() { Date = DateOnly.FromDateTime(DateTime.Today), TemperatureC = 0, Summary = "Mild" }
        };
        _repositoryMock.Setup(r => r.GetForecasts(1)).Returns(fakeData);

        var result = _service.GetForecasts(1).Single();

        // 0°C → 32°F
        Assert.Equal(32, result.TemperatureF);
    }

    [Fact]
    public void GetForecasts_DelegatesToRepository()
    {
        _repositoryMock.Setup(r => r.GetForecasts(5)).Returns([]);

        _service.GetForecasts(5);

        _repositoryMock.Verify(r => r.GetForecasts(5), Times.Once);
    }

    [Fact]
    public void GetForecasts_EmptyRepository_ReturnsEmpty()
    {
        _repositoryMock.Setup(r => r.GetForecasts(It.IsAny<int>())).Returns([]);

        var result = _service.GetForecasts(10);

        Assert.Empty(result);
    }
}

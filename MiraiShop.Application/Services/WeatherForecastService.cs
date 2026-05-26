using MiraiShop.Application.DTOs;
using MiraiShop.Application.Interfaces;
using MiraiShop.Domain.Interfaces;

namespace MiraiShop.Application.Services;

public class WeatherForecastService : IWeatherForecastService
{
    private readonly IWeatherForecastRepository _repository;

    public WeatherForecastService(IWeatherForecastRepository repository)
    {
        _repository = repository;
    }

    public IEnumerable<WeatherForecastDto> GetForecasts(int count)
    {
        return _repository.GetForecasts(count)
            .Select(f => new WeatherForecastDto(f.Date, f.TemperatureC, f.TemperatureF, f.Summary));
    }
}

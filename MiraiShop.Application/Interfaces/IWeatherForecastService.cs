using MiraiShop.Application.DTOs;

namespace MiraiShop.Application.Interfaces;

public interface IWeatherForecastService
{
    IEnumerable<WeatherForecastDto> GetForecasts(int count);
}

using MiraiShop.Domain.Entities;

namespace MiraiShop.Domain.Interfaces;

public interface IWeatherForecastRepository
{
    IEnumerable<WeatherForecast> GetForecasts(int count);
}

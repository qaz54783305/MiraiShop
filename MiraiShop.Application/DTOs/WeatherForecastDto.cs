namespace MiraiShop.Application.DTOs;

public record WeatherForecastDto(
    DateOnly Date,
    int TemperatureC,
    int TemperatureF,
    string? Summary);

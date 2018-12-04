using System.Collections.Generic;
using MyList.Models;

namespace MyList.Providers
{
    public interface IWeatherProvider
    {
        List<WeatherForecast> GetForecasts();
    }
}

using WeatherBot.Models;

namespace WeatherBot.Repositories
{
    public interface IWeatherRepository
    {
        Task<List<WeatherHistory>> GetAllWeatherHistory();
        Task SaveWeather(WeatherHistory weatherHistory);
        Task<List<WeatherHistory>> GetWeatherHistoryByUserId(int userId);
    }
}

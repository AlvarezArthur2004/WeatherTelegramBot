using Dapper;
using System.Data;
using WeatherBot.Models;

namespace WeatherBot.Repositories
{
    public class WeatherRepository : IWeatherRepository
    {
        private readonly IDbConnection _dbConnection;

        public WeatherRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<List<WeatherHistory>> GetAllWeatherHistory()
        {
            var query = "SELECT * FROM WeatherHistory";
            var weatherHistory = await _dbConnection.QueryAsync<WeatherHistory>(query);
            return weatherHistory.ToList();
        }

        public async Task<List<WeatherHistory>> GetWeatherHistoryByUserId(int userId)
        {
            var query = "SELECT * FROM WeatherHistory WHERE UserId = @UserId";
            var weatherHistory = await _dbConnection.QueryAsync<WeatherHistory>(query, new { UserId = userId });
            return weatherHistory.ToList();
        }

        public async Task SaveWeather(WeatherHistory weatherHistory)
        {
            var query = @"INSERT INTO WeatherHistory (UserId, Location, WeatherTokenApi) 
                          VALUES (@UserId, @Location, @WeatherTokenApi)";
            await _dbConnection.ExecuteAsync(query, weatherHistory);
        }
    }
}

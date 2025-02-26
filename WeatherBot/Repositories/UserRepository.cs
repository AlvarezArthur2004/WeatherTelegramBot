using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using Telegram.Bot.Types;
using WeatherBot.Models;

namespace WeatherBot.Repositories
{
    public class UserRepository : IUserRepository
    {

        private readonly IDbConnection _dbConnection;

        public UserRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<List<WeatherUser>> GetAllUser()
        {
            var users = await _dbConnection.QueryAsync<WeatherUser>("SELECT * FROM Users");
            return users.ToList();
        }

        public async Task<WeatherUser?> GetUserByTelegramId(long telegramId)
        {
            var user = await _dbConnection.QueryFirstOrDefaultAsync<WeatherUser>(
                "SELECT * FROM Users WHERE TelegramId = @TelegramId",
                new { TelegramId = telegramId }
            );
            return user;
        }

        public async Task AddUser(long telegramId, long chatId, string currentLocation)
        {
            var query = "INSERT INTO Users (TelegramId, ChatId, CurrentLocation, CreatedAt) VALUES (@TelegramId, @ChatId, @CurrentLocation, GETDATE())";
            await _dbConnection.ExecuteAsync(query, new { TelegramId = telegramId, ChatId = chatId, CurrentLocation = currentLocation });
        }

        public async Task<bool> UserExists(long telegramId)
        {
            var query = "SELECT COUNT(*) FROM Users WHERE TelegramId = @TelegramId";
            var count = await _dbConnection.ExecuteScalarAsync<int>(query, new { TelegramId = telegramId });
            return count > 0;
        }

        public async Task UpdateUserLocation(long telegramId, string location)
        {
            var query = "UPDATE Users SET CurrentLocation = @Location WHERE TelegramId = @TelegramId";
            await _dbConnection.ExecuteAsync(query, new { Location = location, TelegramId = telegramId });
        }
    }
}

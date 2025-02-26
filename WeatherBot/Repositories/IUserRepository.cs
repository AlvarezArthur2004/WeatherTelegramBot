
using WeatherBot.Models;

namespace WeatherBot.Repositories
{
    public interface IUserRepository
    {
        Task<List<WeatherUser>> GetAllUser();
        Task<WeatherUser?> GetUserByTelegramId(long telegramId);
        Task AddUser(long telegramId, long chatId, string currentLocation);
        Task<bool> UserExists(long telegramId);
        Task UpdateUserLocation(long telegramId, string location);
    }
}

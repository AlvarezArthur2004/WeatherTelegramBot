using Telegram.Bot.Types;

namespace WeatherBot.Models
{
    public class WeatherHistory
    {
        public int Id { get; set; }
        public required int UserId { get; set; } 
        public required string Location { get; set; }
        public required string WeatherTokenApi { get; set; }
        public virtual WeatherUser User { get; set; } = null!;
    }
}
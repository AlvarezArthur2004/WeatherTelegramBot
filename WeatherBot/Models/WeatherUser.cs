namespace WeatherBot.Models
{
    public class WeatherUser
    {
        public int Id { get; set; }
        public required long TelegramId { get; set; }
        public required long ChatId { get; set; }
        public required string CurrentLocation { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}



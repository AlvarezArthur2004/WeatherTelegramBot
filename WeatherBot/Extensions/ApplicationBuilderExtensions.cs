using WeatherBot.Services;

namespace WeatherBot.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static async Task InitializeDatabaseAsync(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var dbInitializer = scope.ServiceProvider.GetRequiredService<TelegramWeatherBotDbInitializer>();
            await dbInitializer.InitializeDatabase();
        }

        public static void StartTelegramBot(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var botService = scope.ServiceProvider.GetRequiredService<TelegramBotService>();
            botService.Start();
        }
    }
}

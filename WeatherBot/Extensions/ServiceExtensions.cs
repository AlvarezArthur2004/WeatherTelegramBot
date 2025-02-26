using Microsoft.Data.SqlClient;
using System.Data;
using WeatherBot.Repositories;
using WeatherBot.Services;

namespace WeatherBot.Extensions
{
    public static class ServiceExtensions
    {
        public static void ConfigureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            var connectionString = configuration.GetConnectionString("WeatherDefaultDbConnection");
            services.AddSingleton<IDbConnection>(sp => new SqlConnection(connectionString));

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IWeatherRepository, WeatherRepository>();
            services.AddScoped<TelegramWeatherBotDbInitializer>();

            services.AddSingleton<TelegramBotService>(sp =>
            {
                var botToken = configuration["TelegramBot:Token"]
                    ?? throw new ArgumentNullException("TelegramBot:Token is missing");

                var scopeFactory = sp.GetRequiredService<IServiceScopeFactory>();

                return new TelegramBotService(botToken, configuration, scopeFactory);
            });
        }
    }
}

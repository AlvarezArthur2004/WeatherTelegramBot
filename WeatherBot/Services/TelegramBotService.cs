using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using WeatherBot.Repositories;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Polling;
using System.Text.Json;
using WeatherBot.Models;

namespace WeatherBot.Services
{
    public class TelegramBotService 
    {
        private readonly ITelegramBotClient _botClient;
        private CancellationTokenSource? _cts;
        private readonly IServiceScopeFactory _scopeFactory;

        private readonly string _apiKey;
        private readonly string _weatherUrl;

        public TelegramBotService(string botToken, IConfiguration configuration, IServiceScopeFactory scopeFactory)
        {
            _botClient = new TelegramBotClient(botToken);
            _scopeFactory = scopeFactory;
            _apiKey = configuration["WeatherAPI:apiKey"] ?? throw new ArgumentNullException("WeatherAPI:apiKey is missing");
            _weatherUrl = configuration["WeatherAPI:WeatherURL"] ?? throw new ArgumentNullException("WeatherAPI:WeatherURL is missing");
        }

        private async Task SetBotCommands()
        {
            var commands = new[]
            {
                new BotCommand { Command = "/weather", Description = "[City] Get a weather forecast" },
                new BotCommand { Command = "/help", Description = "Show list of commands." }
            };

            await _botClient.SetMyCommands(commands);
        }

        public async void Start()
        {
            _cts = new CancellationTokenSource();
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>()
            };

            await SetBotCommands();

            _botClient.StartReceiving(
                HandleUpdate,
                HandlePollingError,
                receiverOptions,
                _cts.Token
            );

            Console.WriteLine("Bot started...");
        }

        public void Stop()
        {
            _cts?.Cancel();
            Console.WriteLine("Bot stopped...");
        }

        private async Task HandleUpdate(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Type == UpdateType.Message && update.Message?.Text != null)
            {
                var message = update.Message;
                long chatId = message.Chat.Id;
                string text = message.Text.Trim();
                long telegramId = message.From.Id;

                using (var scope = _scopeFactory.CreateScope())
                {
                    var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

                    var userExists = await userRepository.UserExists(telegramId);
                    if (userExists)
                    {
                        var user = await userRepository.GetUserByTelegramId(telegramId);
                        if (string.IsNullOrEmpty(user.CurrentLocation))
                        {
                            await userRepository.UpdateUserLocation(telegramId, text);
                            await _botClient.SendMessage(chatId,
                                $"Thanks! 🌍 Your city is set to *{text}*.\nYou can now check the weather with /weather.");
                            return;
                        }
                    }
                }

                switch (text.Split(' ')[0])
                {
                    case "/start":
                        await HandleStartCommand(message, cancellationToken);
                        break;

                    case "/weather":
                        await HandleWeatherCommand(chatId, text);
                        break;

                    case "/help":
                        await HandleHelpCommand(chatId, cancellationToken);
                        break;

                    default:
                        await _botClient.SendMessage(chatId,
                            "🌦️ Oops! It looks like you've entered an incorrect command.\n" +
                            "Please check your input and try again. If you need help, type /help.");
                        break;
                }
            }
        }

        private async Task HandleStartCommand(Message message, CancellationToken cancellationToken)
        {
            long chatId = message.Chat.Id;
            long telegramId = message.From.Id;
            string firstName = message.From.FirstName ?? "there";

            using (var scope = _scopeFactory.CreateScope())
            {
                var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

                var userExists = await userRepository.UserExists(telegramId);
                if (!userExists)
                {
                    await userRepository.AddUser(telegramId, chatId, ""); 
                    await _botClient.SendMessage(chatId,
                        "Hello, " + firstName + "! 👋\nI'm your personal weather assistant. 🌤️\n\n" +
                        "Please enter your city to receive accurate weather forecasts.");
                }
                else
                {
                    await _botClient.SendMessage(chatId,
                        $"Welcome back, {firstName}! 🌤️ You can request the weather with /weather [city].");
                }
            }
        }



        private async Task HandleHelpCommand(long chatId, CancellationToken cancellationToken)
        {
            string helpMessage = "🌟 *Available Commands:* 🌟\n\n" +
                                 "✅ `/weather [city]` - Get the current weather for a specified city.\n" +
                                 "✅ `/help` - Show this list of commands.\n\n" +
                                 "💡 Example: `/weather London`\n\n";

            await _botClient.SendMessage(chatId, helpMessage, cancellationToken: cancellationToken);
        }

        public async Task HandleWeatherCommand(long chatId, string text)
        {
            string city = text.Replace("/weather", "").Trim();
            if (string.IsNullOrEmpty(city))
            {
                await _botClient.SendMessage(chatId, "⚠️ Usage: /weather (city)");
                return;
            }

            string apiUrl = $"{_weatherUrl}?q={city}&appid={_apiKey}&units=metric";

            using HttpClient client = new HttpClient();
            try
            {
                HttpResponseMessage response = await client.GetAsync(apiUrl);
                if (!response.IsSuccessStatusCode)
                {
                    await _botClient.SendMessage(chatId, $"❌ Unable to get the weather forecast for {city}. Please check the city name and try again.");
                    return;
                }

                string jsonResponse = await response.Content.ReadAsStringAsync();
                using JsonDocument doc = JsonDocument.Parse(jsonResponse);
                JsonElement root = doc.RootElement;
                        string cityName = root.GetProperty("name").GetString();
                        double temp = root.GetProperty("main").GetProperty("temp").GetDouble();
                        double feelsLike = root.GetProperty("main").GetProperty("feels_like").GetDouble();
                        int humidity = root.GetProperty("main").GetProperty("humidity").GetInt32();
                        int pressure = root.GetProperty("main").GetProperty("pressure").GetInt32();
                        string weatherMain = root.GetProperty("weather")[0].GetProperty("main").GetString();
                        string weatherDescription = root.GetProperty("weather")[0].GetProperty("description").GetString();
                        int cloudiness = root.GetProperty("clouds").GetProperty("all").GetInt32();
                        double windSpeed = root.GetProperty("wind").GetProperty("speed").GetDouble();
                        int windDirection = root.GetProperty("wind").GetProperty("deg").GetInt32();

                        string weatherMessage = $"🌍 *Weather forecast for {cityName}* 🌍\n" +
                                                $"☁️ Cloudiness: {cloudiness}%\n" +
                                                $"🌤 Condition: {weatherMain} ({weatherDescription})\n" +
                                                $"🌡 Temperature: {temp}°C (feels like {feelsLike}°C)\n" +
                                                $"💧 Humidity: {humidity}%  🔵 Pressure: {pressure} hPa\n" +
                                                $"💨 Wind: {windSpeed} m/s, direction {windDirection}°\n\n" +
                                                $"Be prepared for the weather and have a great day! 😊";

                        await _botClient.SendMessage(chatId, weatherMessage);

                using (var scope = _scopeFactory.CreateScope())
                {
                    var weatherRepository = scope.ServiceProvider.GetRequiredService<IWeatherRepository>();
                    var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

                    var user = await userRepository.GetUserByTelegramId(chatId);
                    if (user != null)
                    {
                        var weatherHistory = new WeatherHistory
                        {
                            UserId = user.Id,
                            Location = cityName,
                            WeatherTokenApi = apiUrl
                        };

                        await weatherRepository.SaveWeather(weatherHistory);
                    }
                }
            }
            catch (Exception ex)
            {
                await _botClient.SendMessage(chatId, $"❌ Error retrieving weather data: {ex.Message}");
            }
        }

        private Task HandlePollingError(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Error: {exception.Message}");
            return Task.CompletedTask;
        }

        public async Task SendMessage(long chatId, string message)
        {
            await _botClient.SendMessage(chatId, message);
        }

        public async Task AddUser(long chatId, string message)
        {
            await _botClient.SendMessage(chatId, message);
        }


    }
}

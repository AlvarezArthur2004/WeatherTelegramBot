using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;
using WeatherBot.Repositories;
using WeatherBot.Services;

namespace WeatherBot.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TelegramController : ControllerBase
    {
        private readonly TelegramBotService _telegramBotService;
        private readonly IUserRepository _userRepository;

        public TelegramController(TelegramBotService telegramBotService, IUserRepository userRepository)
        {
            _telegramBotService = telegramBotService;
            _userRepository = userRepository;
        }

        [HttpPost("SendMessage")]
        public async Task<IActionResult> SendMessage(long chatId, string message)
        {
            await _telegramBotService.SendMessage(chatId, message);
            return Ok("Message was send");
        }

        [HttpPost("SendMessageToAll")]
        public async Task<IActionResult> SendMessageToAll(string message)
        {
            var users = await _userRepository.GetAllUser();
            if (users == null || !users.Any())
                return NotFound("No users found.");

            foreach (var user in users)
            {
                await _telegramBotService.SendMessage(user.ChatId, message);
            }

            return Ok("Message sent to all users.");
        }


        [HttpPost("SendWeatherToAll")]
        public async Task<IActionResult> SendWeatherToAll([FromServices] TelegramBotService weatherService)
        {
            var users = await _userRepository.GetAllUser();
            if (users == null || !users.Any())
                return NotFound("No users found.");

            foreach (var user in users)
            {
                if (string.IsNullOrEmpty(user.CurrentLocation))
                {
                    await _telegramBotService.SendMessage(user.ChatId, "We do not have a location for you. Please set your city first.");
                    continue;
                }

                string city = user.CurrentLocation;
                long chatId = user.ChatId;

                await weatherService.HandleWeatherCommand(chatId, city);
            }

            return Ok("Weather message sent to all users.");
        }

        [HttpPost("SendWeatherById")]
        public async Task<IActionResult> SendWeatherById(long chatId, [FromServices] TelegramBotService weatherService)
        {
            var user = await _userRepository.GetUserByTelegramId(chatId);
            if (user == null)
                return NotFound("No users found.");

            string city = user.CurrentLocation;

            await weatherService.HandleWeatherCommand(chatId, city);

            return Ok("Message sent to user.");
        }

    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WeatherBot.Models;
using WeatherBot.Repositories;

namespace WeatherBot.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WeatherHistoryController : ControllerBase
    {
        private readonly IWeatherRepository _weatherHistoryRepository;

        public WeatherHistoryController(IWeatherRepository weatherHistoryRepository)
        {
            _weatherHistoryRepository = weatherHistoryRepository;
        }


        [HttpGet("{userId}")]
        public async Task<IActionResult> GetWeatherHistoryByUser(int userId)
        {
            var history = await _weatherHistoryRepository.GetWeatherHistoryByUserId(userId);
            if (history == null || !history.Any())
                return NotFound("The user either does not exist or has not entered any information");

            return Ok(history);
        }

        [HttpGet]
        public async Task<ActionResult<List<WeatherUser>>> GetAllUsers()
        {
            var weatherUser = await _weatherHistoryRepository.GetAllWeatherHistory();
            return Ok(weatherUser);
        }


    }
}

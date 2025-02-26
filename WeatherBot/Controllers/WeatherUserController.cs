using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WeatherBot.Models;
using WeatherBot.Repositories;

namespace WeatherBot.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WeatherUserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;

        public WeatherUserController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpGet]
        public async Task<ActionResult<List<WeatherUser>>> GetAllUsers()
        {
            var weatherUser = await _userRepository.GetAllUser();
            return Ok(weatherUser);
        }
    }
}

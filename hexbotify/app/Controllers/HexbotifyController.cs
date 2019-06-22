using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Hexbotify.Services;

namespace Hexbotify.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HexbotifyController : ControllerBase
    {
        private readonly IHexbotifier _hexbotifier;

        public HexbotifyController(IHexbotifier hexbotifier)
        {
            _hexbotifier = hexbotifier;
        }

        [HttpGet]
        public async Task<ActionResult<dynamic>> Get(int? count, int? width, int? height, string seed, string imageUrl)
        {
            return await _hexbotifier.Go(count, width, height, seed, imageUrl);
        }
    }
}

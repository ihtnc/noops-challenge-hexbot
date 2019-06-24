using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Hexbotify.Services;

namespace Hexbotify.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HexbotifyController : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult> Get([FromServices] IHexbotifier hexbotifier, int? count = null, int? width = null, int? height = null, string seed = null, string canvas = null, bool? animate = null)
        {
            var response = await hexbotifier.Go(count, width, height, seed, canvas, animate);
            return new FileContentResult(response.Image, response.ContentType);
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TwitchBot.Api.Helpers;

using TwitchBotDb.Context;
using TwitchBotDb.Models;

namespace TwitchBot.Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class TwitchGameCategoriesController : ControllerBase
    {
        private readonly SimpleBotContext _context;

        public TwitchGameCategoriesController(SimpleBotContext context)
        {
            _context = context;
        }

        // GET: api/twitchgamecategories/get
        // GET: api/twitchgamecategories/get?title=IRL
        [HttpGet()]
        public async Task<IActionResult> Get([FromQuery] string title)
        {
            if (string.IsNullOrEmpty(title))
            {
                return Ok(_context.TwitchGameCategories);
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            TwitchGameCategory? gameList = await _context.TwitchGameCategories
                .FirstOrDefaultAsync(m => m.Title == title);

            if (gameList == null)
            {
                throw new NotFoundException("Game list cannot be found");
            }

            return Ok(gameList);
        }
    }
}

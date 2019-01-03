using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TwitchBotDb.Models;

namespace TwitchBotApi.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]/[action]")]
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
                return Ok(_context.TwitchGameCategory);
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            TwitchGameCategory gameList = await _context.TwitchGameCategory.FirstOrDefaultAsync(m => m.Title == title);

            if (gameList == null)
            {
                return NotFound();
            }

            return Ok(gameList);
        }
    }
}
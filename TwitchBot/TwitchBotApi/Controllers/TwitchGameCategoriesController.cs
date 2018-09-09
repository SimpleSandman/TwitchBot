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
        [HttpGet]
        public IEnumerable<TwitchGameCategory> Get()
        {
            return _context.TwitchGameCategory;
        }

        // GET: api/twitchgamecategories/get/IRL
        [HttpGet("{title}")]
        public async Task<IActionResult> Get([FromRoute] string title)
        {
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
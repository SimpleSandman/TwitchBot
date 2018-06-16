using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TwitchBotDb.Models;

namespace TwitchBotApi.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]/[action]")]
    public class GameListsController : ControllerBase
    {
        private readonly TwitchBotDbContext _context;

        public GameListsController(TwitchBotDbContext context)
        {
            _context = context;
        }

        // GET: api/gamelists/get
        [HttpGet]
        public IEnumerable<GameList> Get()
        {
            return _context.GameList;
        }

        // GET: api/gamelists/get/5
        [HttpGet("{name}")]
        public async Task<IActionResult> Get([FromRoute] string name)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            GameList gameList = await _context.GameList.FirstOrDefaultAsync(m => m.Name == name);

            if (gameList == null)
            {
                return NotFound();
            }

            return Ok(gameList);
        }
    }
}
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TwitchBotDb.Context;
using TwitchBotDb.Models;

namespace TwitchBotApi.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]/[action]")]
    public class BotModeratorsController : ControllerBase
    {
        private readonly SimpleBotContext _context;

        public BotModeratorsController(SimpleBotContext context)
        {
            _context = context;
        }

        // GET: api/botmoderators/get/2
        // GET: api/botmoderators/get/2?username=simple_sandman
        [HttpGet("{broadcasterId:int}")]
        public async Task<IActionResult> Get([FromRoute] int broadcasterId, [FromQuery] string username = "")
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var botModerators = new object();

            if (string.IsNullOrEmpty(username))
                botModerators = await _context.BotModerators.Where(m => m.BroadcasterId == broadcasterId).ToListAsync();
            else
                botModerators = await _context.BotModerators.SingleOrDefaultAsync(m => m.BroadcasterId == broadcasterId && m.Username == username);

            if (botModerators == null)
            {
                return NotFound();
            }

            return Ok(botModerators);
        }

        // PUT: api/botmoderators/5?broadcasterId=2
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromQuery] int broadcasterId, [FromBody] BotModerator botModerator)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != botModerator.Id || broadcasterId != botModerator.BroadcasterId)
            {
                return BadRequest();
            }

            _context.Entry(botModerator).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BotModeratorExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/botmoderators/create
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] BotModerator botModerator)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.BotModerators.Add(botModerator);
            await _context.SaveChangesAsync();

            return CreatedAtAction("Get", new { broadcasterId = botModerator.BroadcasterId, username = botModerator.Username }, botModerator);
        }

        // DELETE: api/botmoderators/delete/5?username=simple_sandman
        [HttpDelete("{broadcasterId:int}")]
        public async Task<IActionResult> Delete([FromRoute] int broadcasterId, [FromQuery] string username)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            BotModerator botModerator = await _context.BotModerators.SingleOrDefaultAsync(m => m.BroadcasterId == broadcasterId && m.Username == username);
            if (botModerator == null)
            {
                return NotFound();
            }

            _context.BotModerators.Remove(botModerator);
            await _context.SaveChangesAsync();

            return Ok(botModerator);
        }

        private bool BotModeratorExists(int id)
        {
            return _context.BotModerators.Any(e => e.Id == id);
        }
    }
}

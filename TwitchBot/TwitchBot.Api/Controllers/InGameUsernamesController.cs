using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TwitchBot.Api.Helpers;

using TwitchBotDb.Context;
using TwitchBotDb.Models;

namespace TwitchBot.Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class InGameUsernamesController : ControllerBase
    {
        private readonly SimpleBotContext _context;

        public InGameUsernamesController(SimpleBotContext context)
        {
            _context = context;
        }

        // GET: api/ingameusernames/get/5
        // GET: api/ingameusernames/get/5?gameId=1
        [HttpGet("{broadcasterId}")]
        public async Task<IActionResult> Get(int broadcasterId, [FromQuery] int? gameId = 0)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            object? inGameUsername = new object();

            if (gameId == 0)
            {
                inGameUsername = await _context.InGameUsernames
                    .Where(m => m.BroadcasterId == broadcasterId).ToListAsync();
            }
            else if (gameId != null)
            {
                inGameUsername = await _context.InGameUsernames
                    .SingleOrDefaultAsync(m => m.BroadcasterId == broadcasterId && m.GameId == gameId);
            }
            else if (gameId == null)
            {
                inGameUsername = await _context.InGameUsernames
                    .SingleOrDefaultAsync(m => m.BroadcasterId == broadcasterId && m.GameId == null);
            }

            if (inGameUsername == null && gameId != null)
            {
                // try getting the generic username message
                inGameUsername = await _context.InGameUsernames
                    .SingleOrDefaultAsync(m => m.BroadcasterId == broadcasterId && m.GameId == null);

                if (inGameUsername == null)
                {
                    throw new NotFoundException("In game name not found");
                }
            }

            return Ok(inGameUsername);
        }

        // PUT: api/ingameusernames/update/2?id=1
        // Body (JSON): { "id": 1, "message": "GenericUsername123", "broadcasterid": 2, "gameid": null }
        // Body (JSON): { "id": 1, "message": "UniqueUsername456", "broadcasterid": 2, "gameid": 2 }
        [HttpPut("{broadcasterId}")]
        public async Task<IActionResult> Update(int broadcasterId, [FromQuery] int id, [FromBody] InGameUsername inGameUsername)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != inGameUsername.Id && broadcasterId != inGameUsername.BroadcasterId)
            {
                throw new ApiException("ID or broadcaster ID does not match with in game name's ID or broadcaster ID");
            }

            _context.Entry(inGameUsername).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!InGameUsernameExists(id))
                {
                    throw new NotFoundException("In game name not found");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/ingameusernames/create
        // Body (JSON): { "message": "GenericUsername123", "broadcasterId": 2 }
        // Body (JSON): { "message": "UniqueUsername456", "broadcasterId": 2, "gameid": 2 }
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] InGameUsername inGameUsername)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.InGameUsernames.Add(inGameUsername);
            await _context.SaveChangesAsync();

            return CreatedAtAction("Get", new { broadcasterId = inGameUsername.BroadcasterId, gameId = inGameUsername.Game }, inGameUsername);
        }

        // DELETE: api/ingameusernames/delete/5?id=2
        [HttpDelete("{broadcasterId}")]
        public async Task<IActionResult> Delete(int broadcasterId, [FromQuery] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            InGameUsername? inGameUsername = await _context.InGameUsernames
                .SingleOrDefaultAsync(m => m.Id == id && m.BroadcasterId == broadcasterId);

            if (inGameUsername == null)
            {
                throw new NotFoundException("In game name not found");
            }

            _context.InGameUsernames.Remove(inGameUsername);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool InGameUsernameExists(int id)
        {
            return _context.InGameUsernames.Any(e => e.Id == id);
        }
    }
}

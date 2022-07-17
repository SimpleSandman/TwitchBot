using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TwitchBot.Api.Helpers.ErrorExceptions;

using TwitchBotDb.Context;

namespace TwitchBot.Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class PartyUpsController : ControllerBase
    {
        private readonly SimpleBotContext _context;

        public PartyUpsController(SimpleBotContext context)
        {
            _context = context;
        }

        // GET: api/partyups/get/2
        // GET: api/partyups/get/2?gameid=2
        // GET: api/partyups/get/2?gameid=2?partymember=Sinon
        [HttpGet("{broadcasterId:int}")]
        public async Task<IActionResult> Get([FromRoute] int broadcasterId, [FromQuery] int gameId = 0, [FromQuery] string partyMember = "")
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            object? partyUp = new object();

            if (gameId > 0 && !string.IsNullOrEmpty(partyMember))
            {
                partyUp = await _context.PartyUps
                    .SingleOrDefaultAsync(m =>
                        m.BroadcasterId == broadcasterId
                            && m.GameId == gameId
                            && m.PartyMemberName.Contains(partyMember, StringComparison.CurrentCultureIgnoreCase));
            }
            else if (gameId > 0)
            {
                partyUp = await _context.PartyUps.Where(m => m.BroadcasterId == broadcasterId && m.GameId == gameId)
                    .Select(m => m.PartyMemberName)
                    .ToListAsync();
            }
            else
            {
                partyUp = await _context.PartyUps.Where(m => m.BroadcasterId == broadcasterId).ToListAsync();
            }

            if (partyUp == null)
            {
                throw new NotFoundException("Party cannot be found");
            }

            return Ok(partyUp);
        }
    }
}

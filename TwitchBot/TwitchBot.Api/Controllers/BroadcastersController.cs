using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TwitchBot.Api.Helpers;
using TwitchBot.Api.Helpers.ErrorExceptions;
using TwitchBotDb.Context;
using TwitchBotDb.Models;

namespace TwitchBot.Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class BroadcastersController : ExtendedControllerBase
    {
        private readonly SimpleBotContext _context;

        public BroadcastersController(SimpleBotContext context)
        {
            _context = context;
        }

        // GET: api/broadcasters/get/12345678
        // GET: api/broadcasters/get/12345678?username=simple_sandman
        [HttpGet("{twitchId}")]
        public async Task<IActionResult> Get(int twitchId, [FromQuery] string username = "")
        {
            IsModelStateValid();

            Broadcaster? broadcaster = new Broadcaster();

            if (!string.IsNullOrEmpty(username))
            {
                broadcaster = await _context.Broadcasters
                    .SingleOrDefaultAsync(m => m.Username == username && m.TwitchId == twitchId);
            }
            else
            {
                broadcaster = await _context.Broadcasters
                    .SingleOrDefaultAsync(m => m.TwitchId == twitchId);
            }

            if (broadcaster == null)
            {
                throw new NotFoundException("Broadcaster not found");
            }

            return Ok(broadcaster);
        }

        // PUT: api/broadcasters/update/12345678
        // Body (JSON): { "username": "simple_sandman", "twitchId": 12345678 }
        [HttpPut("{twitchId}")]
        public async Task<IActionResult> Update(int twitchId, [FromBody] Broadcaster broadcaster)
        {
            IsModelStateValid();

            if (twitchId != broadcaster.TwitchId)
            {
                throw new ApiException("Twitch ID does not match with the broadcaster's Twitch ID");
            }

            Broadcaster? updatedbroadcaster = await _context.Broadcasters
                .FirstOrDefaultAsync(m => m.TwitchId == twitchId);

            if (updatedbroadcaster == null)
            {
                throw new NotFoundException("Broadcaster not found");
            }

            updatedbroadcaster.Username = broadcaster.Username;
            _context.Broadcasters.Update(updatedbroadcaster);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BroadcasterExists(twitchId))
                {
                    throw new NotFoundException("Broadcaster not found");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/broadcasters/create
        // Body (JSON): { "username": "simple_sandman", "twitchId": 12345678 }
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Broadcaster broadcaster)
        {
            IsModelStateValid();

            _context.Broadcasters.Add(broadcaster);
            await _context.SaveChangesAsync();

            return CreatedAtAction("Get", new { twitchId = broadcaster.TwitchId }, broadcaster);
        }

        private bool BroadcasterExists(int twitchId)
        {
            return _context.Broadcasters.Any(e => e.TwitchId == twitchId);
        }
    }
}

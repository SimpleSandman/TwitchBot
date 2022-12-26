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
    public class BossFightBossStatsController : ExtendedControllerBase
    {
        private readonly SimpleBotContext _context;

        public BossFightBossStatsController(SimpleBotContext context)
        {
            _context = context;
        }

        // GET: api/bossfightbossstats/get/1
        // GET: api/bossfightbossstats/get/1?gameId=1
        [HttpGet("{settingsId}")]
        public async Task<IActionResult> Get(int settingsId, [FromQuery] int? gameId = null)
        {
            IsModelStateValid();

            BossFightBossStats? bossFightBossStats = await _context.BossFightBossStats
                .SingleOrDefaultAsync(m => m.SettingsId == settingsId && m.GameId == gameId);

            if (bossFightBossStats == null)
            {
                // User hasn't set the boss stats for a particular game that is in the game list
                // Try to get their general settings as a fallback
                bossFightBossStats = await _context.BossFightBossStats
                    .SingleOrDefaultAsync(m => m.SettingsId == settingsId && m.GameId == null);

                if (bossFightBossStats == null)
                {
                    throw new NotFoundException("Cannot find boss fight boss stats");
                }
            }

            return Ok(bossFightBossStats);
        }

        // PUT: api/bossfightbossstats/update/1?id=1
        [HttpPut("{settingsId}")]
        public async Task<IActionResult> Update(int settingsId, [FromQuery] int id, [FromBody] BossFightBossStats bossFightBossStats)
        {
            IsModelStateValid();

            if (id != bossFightBossStats.Id && settingsId != bossFightBossStats.SettingsId)
            {
                throw new ApiException("Settings ID does not match with boss fight boss stats's settings ID");
            }

            _context.Entry(bossFightBossStats).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BossFightBossStatsExists(id))
                {
                    throw new NotFoundException("Cannot find boss fight boss stats");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/bossfightbossstats/create
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] BossFightBossStats bossFightBossStats)
        {
            IsModelStateValid();

            _context.BossFightBossStats.Add(bossFightBossStats);
            await _context.SaveChangesAsync();

            return CreatedAtAction("Get", new { settingsId = bossFightBossStats.Id, gameId = bossFightBossStats.GameId }, bossFightBossStats);
        }

        private bool BossFightBossStatsExists(int id)
        {
            return _context.BossFightBossStats.Any(e => e.Id == id);
        }
    }
}

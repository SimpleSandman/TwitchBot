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
    public class BossFightBossStatsController : Controller
    {
        private readonly SimpleBotContext _context;

        public BossFightBossStatsController(SimpleBotContext context)
        {
            _context = context;
        }

        // GET: api/bossfightbossstats/get/1
        // GET: api/bossfightbossstats/get/1?gameId=1
        [HttpGet("{settingsId:int}")]
        public async Task<IActionResult> Get([FromRoute] int settingsId, [FromQuery] int? gameId = null)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            BossFightBossStats bossFightBossStats = await _context.BossFightBossStats.SingleOrDefaultAsync(m => m.SettingsId == settingsId && m.GameId == gameId);

            if (bossFightBossStats == null)
            {
                // User hasn't set the boss stats for a particular game that is in the game list
                // Try to get their general settings as a fallback
                bossFightBossStats = await _context.BossFightBossStats.SingleOrDefaultAsync(m => m.SettingsId == settingsId && m.GameId == null);

                if (bossFightBossStats == null)
                    return NotFound();
            }

            return Ok(bossFightBossStats);
        }

        // PUT: api/bossfightbossstats/update/1?id=1
        [HttpPut("{settingsId:int}")]
        public async Task<IActionResult> Update([FromRoute] int settingsId, [FromQuery] int id, [FromBody] BossFightBossStats bossFightBossStats)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != bossFightBossStats.Id && settingsId != bossFightBossStats.SettingsId)
            {
                return BadRequest();
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
                    return NotFound();
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
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.BossFightBossStats.Add(bossFightBossStats);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool BossFightBossStatsExists(int id)
        {
            return _context.BossFightBossStats.Any(e => e.Id == id);
        }
    }
}
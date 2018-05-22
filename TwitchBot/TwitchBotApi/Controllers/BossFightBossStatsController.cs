using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TwitchBotApi.Models;

namespace TwitchBotApi.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]/[action]")]
    public class BossFightBossStatsController : Controller
    {
        private readonly TwitchBotContext _context;

        public BossFightBossStatsController(TwitchBotContext context)
        {
            _context = context;
        }

        // GET: api/bossfightbossstats/get/1?settingsId=1
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get([FromRoute] int id, [FromQuery] int settingsId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            BossFightBossStats bossFightBossStats = await _context.BossFightBossStats.SingleOrDefaultAsync(m => m.Id == id && m.SettingsId == settingsId);

            if (bossFightBossStats == null)
            {
                return NotFound();
            }

            return Ok(bossFightBossStats);
        }

        // PUT: api/bossfightbossstats/update/1?settingsId=1
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromQuery] int settingsId, [FromBody] BossFightBossStats bossFightBossStats)
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
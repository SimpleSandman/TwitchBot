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
    public class BossFightSettingsController : Controller
    {
        private readonly TwitchBotDbContext _context;

        public BossFightSettingsController(TwitchBotDbContext context)
        {
            _context = context;
        }

        // GET: api/bossfightsettings/get/2
        [HttpGet("{broadcasterId:int}")]
        public async Task<IActionResult> Get([FromRoute] int broadcasterId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            BossFightSettings bossFightSettings = await _context.BossFightSettings.SingleOrDefaultAsync(m => m.Broadcaster == broadcasterId);

            if (bossFightSettings == null)
            {
                return NotFound();
            }

            return Ok(bossFightSettings);
        }

        // PUT: api/bossfightsettings/update/2
        [HttpPut("{broadcasterId:int}")]
        public async Task<IActionResult> Update([FromRoute] int broadcasterId, [FromBody] BossFightSettings bossFightSettings)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (broadcasterId != bossFightSettings.Broadcaster)
            {
                return BadRequest();
            }

            _context.Entry(bossFightSettings).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BossFightSettingsExists(broadcasterId))
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

        // POST: api/bossfightsettings/create
        // Body (JSON): { "broadcaster": 2 }
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] BossFightSettings bossFightSettings)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.BossFightSettings.Add(bossFightSettings);
            await _context.SaveChangesAsync();

            return CreatedAtAction("Get", new { broadcasterId = bossFightSettings.Broadcaster }, bossFightSettings);
        }

        private bool BossFightSettingsExists(int broadcasterId)
        {
            return _context.BossFightSettings.Any(e => e.Broadcaster == broadcasterId);
        }
    }
}
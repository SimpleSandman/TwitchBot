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
    public class BossFightSettingsController : Controller
    {
        private readonly TwitchBotContext _context;

        public BossFightSettingsController(TwitchBotContext context)
        {
            _context = context;
        }

        // GET: api/bossfightsettings/get/2/id=1
        [HttpGet("{broadcasterId:int}")]
        public async Task<IActionResult> Get([FromRoute] int broadcasterId, [FromQuery] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            BossFightSettings bossFightSettings = await _context.BossFightSettings.SingleOrDefaultAsync(m => m.Id == id && m.Broadcaster == broadcasterId);

            if (bossFightSettings == null)
            {
                return NotFound();
            }

            return Ok(bossFightSettings);
        }

        // PUT: api/bossfightsettings/update/2/id=1
        [HttpPut("{broadcasterId:int}")]
        public async Task<IActionResult> Update([FromRoute] int broadcasterId, [FromQuery] int id, [FromBody] BossFightSettings bossFightSettings)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != bossFightSettings.Id || broadcasterId != bossFightSettings.Broadcaster)
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
                if (!BossFightSettingsExists(id))
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
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] BossFightSettings bossFightSettings)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.BossFightSettings.Add(bossFightSettings);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool BossFightSettingsExists(int id)
        {
            return _context.BossFightSettings.Any(e => e.Id == id);
        }
    }
}
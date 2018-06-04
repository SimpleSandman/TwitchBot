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
    public class BankHeistSettingsController : Controller
    {
        private readonly TwitchBotDbContext _context;

        public BankHeistSettingsController(TwitchBotDbContext context)
        {
            _context = context;
        }

        // GET: api/bankheistsettings/get/2?id=1
        [HttpGet("{broadcasterId:int}")]
        public async Task<IActionResult> Get([FromRoute] int broadcasterId, [FromQuery] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            BankHeistSettings bankHeistSettings = await _context.BankHeistSettings.SingleOrDefaultAsync(m => m.Id == id && m.Broadcaster == broadcasterId);

            if (bankHeistSettings == null)
            {
                return NotFound();
            }

            return Ok(bankHeistSettings);
        }

        // PUT: api/bankheistsettings/update/2?id=1
        [HttpPut("{broadcasterId:int}")]
        public async Task<IActionResult> Update([FromRoute] int broadcasterId, [FromQuery] int id, [FromBody] BankHeistSettings bankHeistSettings)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != bankHeistSettings.Id || broadcasterId != bankHeistSettings.Broadcaster)
            {
                return BadRequest();
            }

            _context.Entry(bankHeistSettings).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BankHeistSettingsExists(id))
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

        // POST: api/bankheistsettings/create
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] BankHeistSettings bankHeistSettings)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.BankHeistSettings.Add(bankHeistSettings);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool BankHeistSettingsExists(int id)
        {
            return _context.BankHeistSettings.Any(e => e.Id == id);
        }
    }
}
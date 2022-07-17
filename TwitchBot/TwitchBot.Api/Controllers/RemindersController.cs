using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TwitchBot.Api.Helpers.ErrorExceptions;

using TwitchBotDb.Context;
using TwitchBotDb.Models;

namespace TwitchBot.Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class RemindersController : ControllerBase
    {
        private readonly SimpleBotContext _context;

        public RemindersController(SimpleBotContext context)
        {
            _context = context;
        }

        // GET: api/reminders/get/5
        // GET: api/reminders/get/5?id=1
        [HttpGet("{broadcasterId}")]
        public async Task<IActionResult> Get(int broadcasterId, [FromQuery] int id = 0)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            object? reminders = new object();

            if (id == 0)
                reminders = await _context.Reminders.Where(m => m.BroadcasterId == broadcasterId).ToListAsync();
            else
                reminders = await _context.Reminders.SingleOrDefaultAsync(m => m.BroadcasterId == broadcasterId && m.Id == id);

            if (reminders == null)
            {
                throw new NotFoundException("Reminder cannot be found");
            }

            return Ok(reminders);
        }

        // PUT: api/reminders/update/5?broadcasterId=2
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromQuery] int broadcasterId, [FromBody] Reminder reminder)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != reminder.Id && broadcasterId != reminder.BroadcasterId)
            {
                throw new ApiException("ID or broadcaster id does not match reminder's ID or broadcaster ID");
            }

            _context.Entry(reminder).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RemindersExists(id))
                {
                    throw new NotFoundException("Reminder cannot be found");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/reminders/create
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Reminder reminder)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Reminders.Add(reminder);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/reminders/delete/5?broadcasterId=2
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, [FromQuery] int broadcasterId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Reminder? reminder = await _context.Reminders
                .SingleOrDefaultAsync(m => m.Id == id && m.BroadcasterId == broadcasterId);

            if (reminder == null)
            {
                throw new NotFoundException("Reminder cannot be found");
            }

            _context.Reminders.Remove(reminder);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool RemindersExists(int id)
        {
            return _context.Reminders.Any(e => e.Id == id);
        }
    }
}

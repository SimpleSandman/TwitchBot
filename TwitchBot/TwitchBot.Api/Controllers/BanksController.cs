using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Snickler.EFCore;

using TwitchBot.Api.DTO;
using TwitchBot.Api.Helpers;
using TwitchBot.Api.Helpers.ErrorExceptions;
using TwitchBotDb.Context;
using TwitchBotDb.DTO;
using TwitchBotDb.Models;

using TwitchBotShared.Extensions;

namespace TwitchBot.Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class BanksController : ExtendedControllerBase
    {
        private readonly SimpleBotContext _context;

        public BanksController(SimpleBotContext context)
        {
            _context = context;
        }

        // GET: api/banks/get/2
        // GET: api/banks/get/2?username=simple_sandman
        [HttpGet("{broadcasterId}")]
        public async Task<IActionResult> Get(int broadcasterId, [FromQuery] string? username = null)
        {
            IsModelStateValid();

            List<Bank> bank = new List<Bank>();

            if (!string.IsNullOrEmpty(username))
            {
                bank = await _context.Banks
                    .Where(m => m.Broadcaster == broadcasterId && m.Username == username)
                    .ToListAsync();
            }
            else
            {
                bank = await _context.Banks
                    .Where(m => m.Broadcaster == broadcasterId)
                    .ToListAsync();
            }

            if (bank == null || bank.Count == 0)
            {
                throw new NotFoundException("Bank not found");
            }

            return Ok(bank);
        }

        // GET: api/banks/getleaderboard/2?broadcastername=simple_sandman&botname=sandpaibot&topnumber=5
        [HttpGet("{broadcasterId}")]
        public async Task<IActionResult> GetLeaderboard(int broadcasterId, [FromQuery] BroadcasterConfig broadcasterConfig, [FromQuery] int topNumber = 3)
        {
            IsModelStateValid();

            List<Bank> bank = await _context.Banks
                .Where(m => m.Broadcaster == broadcasterId
                    && m.Username != broadcasterConfig.BroadcasterName
                    && m.Username != broadcasterConfig.BotName)
                .OrderByDescending(m => m.Wallet)
                .Take(topNumber)
                .ToListAsync();

            if (bank == null || bank.Count == 0)
            {
                throw new NotFoundException("Bank leaderboard not found");
            }

            return Ok(bank);
        }

        // PUT: api/banks/updateaccount/2?updatedwallet=5000&username=simple_sandman
        [HttpPut("{broadcasterId}")]
        public async Task<IActionResult> UpdateAccount(int broadcasterId, [FromQuery] int updatedWallet, [FromQuery] string username)
        {
            IsModelStateValid();

            Bank? bankAccount = await _context.Banks
                .FirstOrDefaultAsync(t => t.Broadcaster == broadcasterId && t.Username == username);

            if (bankAccount == null)
            {
                throw new ApiException("Bank account cannot be updated");
            }

            bankAccount.Wallet = updatedWallet;

            _context.Banks.Update(bankAccount);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PUT: api/banks/updatecreateaccount/2?deposit=5000&showOutput=true
        // Body (JSON): ["simple_sandman", "user1", "user2"]
        [HttpPut("{broadcasterId}")]
        public async Task<IActionResult> UpdateCreateAccount(int broadcasterId, [FromQuery] int deposit, [FromQuery] bool showOutput, [FromBody] List<string> usernames)
        {
            IsModelStateValid();

            List<BalanceResult> results = new List<BalanceResult>();

            await _context.LoadStoredProc("dbo.UpdateCreateBalance")
                .WithSqlParam("tvpUsernames", usernames.ToDataTable())
                .WithSqlParam("intDeposit", deposit)
                .WithSqlParam("intBroadcasterID", broadcasterId)
                .WithSqlParam("bitShowOutput", showOutput)
                .ExecuteStoredProcAsync((handler) =>
                {
                    results = handler.ReadToList<BalanceResult>().ToList();
                });

            return Ok(results);
        }

        // POST: api/banks/createaccount
        // Body (JSON): { "username": "user1", "wallet": 500, "broadcaster": 2 }
        [HttpPost]
        public async Task<IActionResult> CreateAccount([FromBody] Bank bank)
        {
            IsModelStateValid();

            if (BankExists(bank.Username, bank.Broadcaster))
            {
                throw new ApiException("Bank account already exists");
            }

            _context.Banks.Add(bank);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool BankExists(string username, int broadcasterId)
        {
            return _context.Banks.Any(e => e.Username == username && e.Broadcaster == broadcasterId);
        }
    }
}

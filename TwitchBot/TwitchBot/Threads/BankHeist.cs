using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using TwitchBot.Configuration;
using TwitchBot.Libraries;
using TwitchBot.Models;
using TwitchBot.Services;

namespace TwitchBot.Threads
{
    public class BankHeist
    {
        private IrcClient _irc;
        private string _connStr;
        private int _broadcasterId;
        private Thread _thread;
        private BankService _bank;
        private TwitchBotConfigurationSection _botConfig;
        private BankHeistSettings _heistSettings = BankHeistSettings.Instance;

        public BankHeist() { }

        public BankHeist(string connStr, BankService bank, TwitchBotConfigurationSection botConfig)
        {
            _connStr = connStr;
            _thread = new Thread(new ThreadStart(this.Run));
            _bank = bank;
            _botConfig = botConfig;
        }

        public void Start(IrcClient irc, int broadcasterId)
        {
            _irc = irc;
            _broadcasterId = broadcasterId;
            _heistSettings.CooldownTimePeriod = DateTime.Now;
            _heistSettings.Robbers = new BlockingCollection<BankRobber>();

            _thread.IsBackground = true;
            _thread.Start();
        }

        private void Run()
        {
            while (true)
            {
                if (_heistSettings.IsHeistOnCooldown())
                {
                    double cooldownTime = (_heistSettings.CooldownTimePeriod.Subtract(DateTime.Now)).TotalMilliseconds;
                    Thread.Sleep((int)cooldownTime);
                    _irc.SendPublicChatMessage(_heistSettings.CooldownOver);
                }
                else if (_heistSettings.Robbers.Count > 0 && _heistSettings.IsEntryPeriodOver())
                {
                    _heistSettings.Robbers.CompleteAdding();
                    Consume();

                    // refresh the list and reset the cooldown time period
                    _heistSettings.Robbers = new BlockingCollection<BankRobber>();
                    _heistSettings.CooldownTimePeriod = DateTime.Now.AddMinutes(_heistSettings.CooldownTimePeriodMinutes);
                    _heistSettings.ResultsMessage = "The heist payouts are: ";
                }

                Thread.Sleep(100);
            }
        }

        public void Produce(BankRobber robber)
        {
            _heistSettings.Robbers.Add(robber);
        }

        public void Consume()
        {
            BankHeistLevel heistLevel = _heistSettings.Levels[HeistLevel() - 1];
            BankHeistPayout payout = _heistSettings.Payouts[HeistLevel() - 1];

            _irc.SendPublicChatMessage(_heistSettings.GameStart
                .Replace("@bankname@", heistLevel.LevelBankName));

            Thread.Sleep(2000); // wait in anticipation

            Random rnd = new Random();
            int chance = rnd.Next(1, 101); // 1 - 100

            if (chance >= payout.SuccessRate) // failed
            {
                if (_heistSettings.Robbers.Count == 1)
                {
                    _irc.SendPublicChatMessage(_heistSettings.SingleUserFail
                        .Replace("user@", _heistSettings.Robbers.First().Username)
                        .Replace("@bankname@", heistLevel.LevelBankName));
                }
                else
                {
                    _irc.SendPublicChatMessage(_heistSettings.Success0);
                }

                return;
            }
            
            int numWinners = (int)Math.Ceiling(_heistSettings.Robbers.Count * (payout.SuccessRate / 100));
            IEnumerable<BankRobber> winners = _heistSettings.Robbers.OrderBy(x => rnd.Next()).Take(numWinners);

            foreach (BankRobber winner in winners)
            {
                int funds = _bank.CheckBalance(winner.Username.ToLower(), _broadcasterId);
                decimal earnings = Math.Ceiling(winner.Gamble * payout.WinMultiplier);

                _bank.UpdateFunds(winner.Username.ToLower(), _broadcasterId, (int)earnings + funds);

                _heistSettings.ResultsMessage += $" @{winner.Username} ({(int)earnings} {_botConfig.CurrencyType}),";
            }

            // remove extra ","
            _heistSettings.ResultsMessage = _heistSettings.ResultsMessage.Remove(_heistSettings.ResultsMessage.LastIndexOf(','), 1);

            decimal numWinnersPercentage = numWinners / (decimal)_heistSettings.Robbers.Count;

            // display success outcome
            if (winners.Count() == 1)
            {
                BankRobber onlyWinner = winners.First();
                int earnings = (int)Math.Ceiling(onlyWinner.Gamble * payout.WinMultiplier);

                _irc.SendPublicChatMessage(_heistSettings.SingleUserSuccess
                    .Replace("user@", onlyWinner.Username)
                    .Replace("@bankname@", heistLevel.LevelBankName)
                    .Replace("@winamount@", earnings.ToString())
                    .Replace("@pointsname@", _botConfig.CurrencyType));
            }
            else if (numWinners == _heistSettings.Robbers.Count)
            {
                _irc.SendPublicChatMessage(_heistSettings.Success100 + " " + _heistSettings.ResultsMessage);
            }
            else if (numWinnersPercentage >= 0.34m)
            {
                _irc.SendPublicChatMessage(_heistSettings.Success34 + " " + _heistSettings.ResultsMessage);
            }
            else if (numWinnersPercentage > 0)
            {
                _irc.SendPublicChatMessage(_heistSettings.Success1 + " " + _heistSettings.ResultsMessage);
            }

            Console.WriteLine(_heistSettings.ResultsMessage);
        }

        public bool HasRobberAlreadyEntered(string username)
        {
            return _heistSettings.Robbers.Any(u => u.Username == username) ? true : false;
        }

        public bool IsEntryPeriodOver()
        {
            return _heistSettings.Robbers.IsAddingCompleted ? true : false;
        }

        public int HeistLevel()
        {
            if (_heistSettings.Robbers.Count <= _heistSettings.Levels[0].MaxUsers)
                return 1;
            else if (_heistSettings.Robbers.Count <= _heistSettings.Levels[1].MaxUsers)
                return 2;
            else if (_heistSettings.Robbers.Count <= _heistSettings.Levels[2].MaxUsers)
                return 3;
            else if (_heistSettings.Robbers.Count <= _heistSettings.Levels[3].MaxUsers)
                return 4;
            else
                return 5;
        }

        public string NextLevelMessage()
        {
            if (_heistSettings.Robbers.Count == _heistSettings.Levels[0].MaxUsers + 1)
                return _heistSettings.NextLevelMessages[0];
            else if (_heistSettings.Robbers.Count == _heistSettings.Levels[1].MaxUsers + 1)
                return _heistSettings.NextLevelMessages[1];
            else if (_heistSettings.Robbers.Count == _heistSettings.Levels[2].MaxUsers + 1)
                return _heistSettings.NextLevelMessages[2];
            else if (_heistSettings.Robbers.Count == _heistSettings.Levels[3].MaxUsers + 1)
                return _heistSettings.NextLevelMessages[3];

            return "";
        }
    }
}

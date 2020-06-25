using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TwitchBot.Configuration;
using TwitchBot.Enums;
using TwitchBot.Libraries;
using TwitchBot.Models;
using TwitchBot.Services;

using TwitchBotDb.DTO;

using TwitchBotUtil.Extensions;

namespace TwitchBot.Commands.Features
{
    /// <summary>
    /// The "Command Subsystem" for the "Bank" feature
    /// </summary>
    public sealed class BankFeature : BaseFeature
    {
        private readonly BankService _bank;
        private readonly BroadcasterSingleton _broadcasterInstance = BroadcasterSingleton.Instance;
        private readonly TwitchChatterList _twitchChatterListInstance = TwitchChatterList.Instance;
        private readonly ErrorHandler _errHndlrInstance = ErrorHandler.Instance;

        public BankFeature(IrcClient irc, TwitchBotConfigurationSection botConfig, BankService bank) : base(irc, botConfig)
        {
            _bank = bank;
            _rolePermission.Add("!deposit", new List<ChatterType> { ChatterType.Moderator });
            _rolePermission.Add("!charge", new List<ChatterType> { ChatterType.Moderator });
            _rolePermission.Add("!points", new List<ChatterType> { ChatterType.Viewer });
            _rolePermission.Add($"!{_botConfig.CurrencyType.ToLower()}", new List<ChatterType> { ChatterType.Viewer });
            _rolePermission.Add("!bonusall", new List<ChatterType> { ChatterType.Moderator });
        }

        public override async Task<bool> ExecCommand(TwitchChatter chatter, string requestedCommand)
        {
            try
            {
                switch (requestedCommand)
                {
                    case "!deposit":
                        await Deposit(chatter);
                        return true;
                    case "!charge":
                        await Charge(chatter);
                        return true;
                    case "!bonusall":
                        await BonusAll(chatter);
                        return true;
                    case "!points":
                        await CheckFunds(chatter);
                        return true;
                    default:
                        if (requestedCommand == $"!{_botConfig.CurrencyType.ToLower()}")
                        {
                            await CheckFunds(chatter);
                            return true;
                        }

                        break;
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "BankFeature", "ExecCommand(TwitchChatter, string)", false, requestedCommand, chatter.Message);
            }

            return false;
        }

        /// <summary>
        /// Gives a set amount of stream currency to user
        /// </summary>
        /// <param name="chatter"></param>
        private async Task Deposit(TwitchChatter chatter)
        {
            try
            {
                List<string> userList = new List<string>();

                foreach (int index in chatter.Message.AllIndexesOf("@"))
                {
                    int lengthUsername = chatter.Message.IndexOf(" ", index) - index - 1;
                    if (lengthUsername < 0)
                        userList.Add(chatter.Message.Substring(index + 1).ToLower());
                    else
                        userList.Add(chatter.Message.Substring(index + 1, lengthUsername).ToLower());
                }

                // Check for valid command
                if (chatter.Message.StartsWith("!deposit @", StringComparison.CurrentCultureIgnoreCase))
                    _irc.SendPublicChatMessage($"Please enter a valid amount to a user @{chatter.DisplayName}");
                // Check if moderator is trying to give streamer currency to themselves
                else if (chatter.Username != _botConfig.Broadcaster.ToLower() && userList.Contains(chatter.Username))
                    _irc.SendPublicChatMessage($"Entire deposit voided. You cannot add {_botConfig.CurrencyType} to your own account @{chatter.DisplayName}");
                else
                {
                    // Check if moderator is trying to give streamer currency to other moderators
                    if (chatter.Username != _botConfig.Broadcaster.ToLower())
                    {
                        foreach (string user in userList)
                        {
                            if (_twitchChatterListInstance.GetUserChatterType(user) == ChatterType.Moderator)
                            {
                                _irc.SendPublicChatMessage($"Entire deposit voided. You cannot add {_botConfig.CurrencyType} to another moderator's account @{chatter.DisplayName}");
                                return;
                            }
                        }
                    }

                    int indexAction = chatter.Message.IndexOf(" ");
                    int deposit = -1;
                    bool isValidDeposit = int.TryParse(chatter.Message.Substring(indexAction, chatter.Message.IndexOf("@") - indexAction - 1), out deposit);

                    // Check if deposit amount is valid
                    if (deposit < 0)
                        _irc.SendPublicChatMessage("Please insert a positive whole amount (no decimals) "
                            + $" or use the !charge command to remove {_botConfig.CurrencyType} from a user");
                    else if (!isValidDeposit)
                        _irc.SendPublicChatMessage($"The deposit wasn't accepted. Please try again with a positive whole amount (no decimals) @{chatter.DisplayName}");
                    else
                    {
                        if (userList.Count > 0)
                        {
                            List<BalanceResult> balResultList = await _bank.UpdateCreateBalance(userList, _broadcasterInstance.DatabaseId, deposit, true);

                            string responseMsg = $"Gave {deposit} {_botConfig.CurrencyType} to ";

                            if (balResultList.Count > 1)
                            {
                                foreach (BalanceResult userResult in balResultList)
                                    responseMsg += $"{userResult.Username}, ";

                                responseMsg = responseMsg.ReplaceLastOccurrence(", ", "");
                            }
                            else if (balResultList.Count == 1)
                            {
                                responseMsg += $"@{balResultList[0].Username} ";

                                if (balResultList[0].ActionType == "UPDATE")
                                    responseMsg += $"and now has {balResultList[0].Wallet} {_botConfig.CurrencyType}!";
                                else if (balResultList[0].ActionType == "INSERT")
                                    responseMsg += $"and can now gamble it all away! Kappa";
                            }
                            else
                                responseMsg = $"Unknown error has occurred in retrieving results. Please check your recipient's {_botConfig.CurrencyType}";

                            _irc.SendPublicChatMessage(responseMsg);
                        }
                        else
                        {
                            _irc.SendPublicChatMessage($"There are no chatters to deposit {_botConfig.CurrencyType} @{chatter.DisplayName}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "BankFeature", "Deposit(TwitchChatter)", false, "!deposit");
            }
        }

        /// <summary>
        /// Takes money away from a user
        /// </summary>
        /// <param name="chatter"></param>
        private async Task Charge(TwitchChatter chatter)
        {
            try
            {
                if (chatter.Message.StartsWith("!charge @", StringComparison.CurrentCultureIgnoreCase))
                    _irc.SendPublicChatMessage($"Please enter a valid amount @{chatter.DisplayName}");
                else
                {
                    int indexAction = chatter.Message.IndexOf(" ");
                    int fee = -1;
                    bool isValidFee = int.TryParse(chatter.Message.Substring(indexAction, chatter.Message.IndexOf("@") - indexAction - 1), out fee);
                    string recipient = chatter.Message.Substring(chatter.Message.IndexOf("@") + 1).ToLower();
                    int wallet = await _bank.CheckBalance(recipient, _broadcasterInstance.DatabaseId);

                    // Check user's bank account exist or has currency
                    if (wallet == -1)
                        _irc.SendPublicChatMessage($"{recipient} is not currently banking with us @{chatter.DisplayName}");
                    else if (wallet == 0)
                        _irc.SendPublicChatMessage($"{recipient} is out of {_botConfig.CurrencyType} @{chatter.DisplayName}");
                    // Check if fee can be accepted
                    else if (fee > 0)
                        _irc.SendPublicChatMessage("Please insert a negative whole amount (no decimal numbers) "
                            + $" or use the !deposit command to add {_botConfig.CurrencyType} to a user's account");
                    else if (!isValidFee)
                        _irc.SendPublicChatMessage($"The fee wasn't accepted. Please try again with negative whole amount (no decimals) @{chatter.DisplayName}");
                    else if (chatter.Username != _botConfig.Broadcaster.ToLower() && _twitchChatterListInstance.GetUserChatterType(recipient) == ChatterType.Moderator)
                        _irc.SendPublicChatMessage($"Entire deposit voided. You cannot remove {_botConfig.CurrencyType} from another moderator's account @{chatter.DisplayName}");
                    else /* Deduct funds from wallet */
                    {
                        wallet += fee;

                        // Zero out account balance if user is being charged more than they have
                        if (wallet < 0)
                            wallet = 0;

                        await _bank.UpdateFunds(recipient, _broadcasterInstance.DatabaseId, wallet);

                        // Prompt user's balance
                        if (wallet == 0)
                            _irc.SendPublicChatMessage($"Charged {fee.ToString().Replace("-", "")} {_botConfig.CurrencyType} to {recipient}"
                                + $"'s account! They are out of {_botConfig.CurrencyType} to spend");
                        else
                            _irc.SendPublicChatMessage($"Charged {fee.ToString().Replace("-", "")} {_botConfig.CurrencyType} to {recipient}"
                                + $"'s account! They only have {wallet} {_botConfig.CurrencyType} to spend");
                    }
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "BankFeature", "Charge(TwitchChatter)", false, "!charge");
            }
        }

        /// <summary>
        /// Check user's account balance
        /// </summary>
        /// <param name="chatter">User that sent the message</param>
        private async Task CheckFunds(TwitchChatter chatter)
        {
            try
            {
                int balance = await _bank.CheckBalance(chatter.Username, _broadcasterInstance.DatabaseId);

                if (balance == -1)
                    _irc.SendPublicChatMessage($"You are not currently banking with us at the moment. Please talk to a moderator about acquiring {_botConfig.CurrencyType}");
                else
                    _irc.SendPublicChatMessage($"@{chatter.DisplayName} currently has {balance} {_botConfig.CurrencyType}");
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "BankFeature", "CheckFunds(TwitchChatter)", false, "![currency name]");
            }
        }

        /// <summary>
        /// Gives every viewer currently watching a set amount of currency
        /// </summary>
        /// <param name="chatter"></param>
        private async Task BonusAll(TwitchChatter chatter)
        {
            try
            {
                // Check for valid command
                if (chatter.Message.StartsWith("!bonusall @", StringComparison.CurrentCultureIgnoreCase))
                    _irc.SendPublicChatMessage($"Please enter a valid amount to a user @{chatter.DisplayName}");
                else
                {
                    int indexAction = chatter.Message.IndexOf(" ");
                    int deposit = -1;
                    bool isValidDeposit = int.TryParse(chatter.Message.Substring(indexAction), out deposit);

                    // Check if deposit amount is valid
                    if (deposit < 0)
                        _irc.SendPublicChatMessage("Please insert a positive whole amount (no decimals) "
                            + $" or use the !charge command to remove {_botConfig.CurrencyType} from a user");
                    else if (!isValidDeposit)
                        _irc.SendPublicChatMessage($"The bulk deposit wasn't accepted. Please try again with positive whole amount (no decimals) @{chatter.DisplayName}");
                    else
                    {
                        // Wait until chatter lists are available
                        while (!_twitchChatterListInstance.AreListsAvailable)
                        {

                        }

                        List<string> chatterList = _twitchChatterListInstance.ChattersByName;

                        // broadcaster gives stream currency to all but themselves and the bot
                        if (chatter.Username == _botConfig.Broadcaster.ToLower())
                        {
                            chatterList = chatterList.Where(t => t != chatter.Username.ToLower() && t != _botConfig.BotName.ToLower()).ToList();
                        }
                        else // moderators gives stream currency to all but other moderators (including broadcaster)
                        {
                            chatterList = chatterList
                                .Where(t => _twitchChatterListInstance.GetUserChatterType(t) != ChatterType.Moderator
                                    && t != _botConfig.BotName.ToLower()).ToList();
                        }

                        if (chatterList != null && chatterList.Count > 0)
                        {
                            await _bank.UpdateCreateBalance(chatterList, _broadcasterInstance.DatabaseId, deposit);

                            _irc.SendPublicChatMessage($"{deposit} {_botConfig.CurrencyType} for everyone! "
                                + $"Check your stream bank account with !{_botConfig.CurrencyType.ToLower()}");
                        }
                        else
                        {
                            _irc.SendPublicChatMessage($"There are no chatters to deposit {_botConfig.CurrencyType} @{chatter.DisplayName}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await _errHndlrInstance.LogError(ex, "BankFeature", "BonusAll(TwitchChatter)", false, "!bonusall");
            }
        }
    }
}

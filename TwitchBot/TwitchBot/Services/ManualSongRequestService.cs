using System.Collections.Generic;
using System.Threading.Tasks;

using TwitchBot.Extensions;
using TwitchBot.Repositories;

using TwitchBotDb.Models;

namespace TwitchBot.Services
{
    public class ManualSongRequestService
    {
        private ManualSongRequestRepository _songRequestDb;

        public ManualSongRequestService(ManualSongRequestRepository songRequestDb)
        {
            _songRequestDb = songRequestDb;
        }

        public async Task<SongRequests> AddSongRequest(string songRequestName, string username, int broadcasterId)
        {
            return await _songRequestDb.AddSongRequest(songRequestName, username, broadcasterId);
        }

        public async Task<string> ListSongRequests(int broadcasterId)
        {
            List<SongRequests> songRequests = await _songRequestDb.ListSongRequests(broadcasterId);

            if (songRequests == null || songRequests.Count == 0)
                return "No song requests have been made";

            string message = "Current list of requested songs: ";

            foreach (SongRequests member in songRequests)
            {
                message += $"\"{member.Requests}\" ({member.Chatter}) >< ";
            }

            return message.ReplaceLastOccurrence(" >< ", "");
        }

        public async Task<SongRequests> PopSongRequest(int broadcasterId)
        {
            return await _songRequestDb.PopSongRequest(broadcasterId);
        }

        public async Task<List<SongRequests>> ResetSongRequests(int broadcasterId)
        {
            return await _songRequestDb.ResetSongRequests(broadcasterId);
        }
    }
}

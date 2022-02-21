using System.Collections.Generic;
using System.Threading.Tasks;

using TwitchBotDb.Models;
using TwitchBotDb.Repositories;

namespace TwitchBotDb.Services
{
    public class ManualSongRequestService
    {
        private readonly ManualSongRequestRepository _songRequestDb;

        public ManualSongRequestService(ManualSongRequestRepository songRequestDb)
        {
            _songRequestDb = songRequestDb;
        }

        public async Task<SongRequest> AddSongRequestAsync(string songRequestName, string username, int broadcasterId)
        {
            return await _songRequestDb.AddSongRequestAsync(songRequestName, username, broadcasterId);
        }

        public async Task<string> ListSongRequestsAsync(int broadcasterId)
        {
            List<SongRequest> songRequests = await _songRequestDb.ListSongRequestsAsync(broadcasterId);

            if (songRequests == null || songRequests.Count == 0)
            {
                return "No song requests have been made";
            }

            string message = "Current list of requested songs: ";

            foreach (SongRequest member in songRequests)
            {
                message += $"\"{member.Name}\" ({member.Username}) >< ";
            }

            message = message.Substring(0, message.Length - 4);

            return message;
        }

        public async Task<SongRequest> PopSongRequestAsync(int broadcasterId)
        {
            return await _songRequestDb.PopSongRequestAsync(broadcasterId);
        }

        public async Task<List<SongRequest>> ResetSongRequestsAsync(int broadcasterId)
        {
            return await _songRequestDb.ResetSongRequestsAsync(broadcasterId);
        }
    }
}

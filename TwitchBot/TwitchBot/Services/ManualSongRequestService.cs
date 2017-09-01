using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchBot.Repositories;

namespace TwitchBot.Services
{
    public class ManualSongRequestService
    {
        private ManualSongRequestRepository _songRequestDb;

        public ManualSongRequestService(ManualSongRequestRepository songRequestDb)
        {
            _songRequestDb = songRequestDb;
        }

        public void AddSongRequest(string songRequestName, string username, int broadcasterId)
        {
            _songRequestDb.AddSongRequest(songRequestName, username, broadcasterId);
        }

        public string ListSongRequests(int broadcasterId)
        {
            return _songRequestDb.ListSongRequests(broadcasterId);
        }

        public string GetFirstSongRequest(int broadcasterId)
        {
            return _songRequestDb.GetFirstSongRequest(broadcasterId);
        }

        public void PopSongRequest(int broadcasterId)
        {
            _songRequestDb.PopSongRequest(broadcasterId);
        }
    }
}

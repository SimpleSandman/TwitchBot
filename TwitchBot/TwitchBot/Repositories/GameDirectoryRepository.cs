using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchBot.Repositories
{
    public class GameDirectoryRepository
    {
        private string _connStr;

        public GameDirectoryRepository(string connStr)
        {
            _connStr = connStr;
        }

        public int GetGameId(string gameTitle, out bool hasMultiplayer)
        {
            int gameId = 0;
            hasMultiplayer = false;

            using (SqlConnection conn = new SqlConnection(_connStr))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT * FROM GameList", conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            if (gameTitle.Equals(reader["Name"].ToString()))
                            {
                                gameId = int.Parse(reader["Id"].ToString());
                                hasMultiplayer = bool.Parse(reader["Multiplayer"].ToString());
                                break;
                            }
                        }
                    }
                }
            }

            return gameId;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TwitchBot.Models;

namespace TwitchBot.Repositories
{
    public class FollowerRepository
    {
        private string _connStr;

        public FollowerRepository(string connStr)
        {
            _connStr = connStr;
        }

        public int CurrExp(string chatter, int broadcasterId)
        {
            using (SqlConnection conn = new SqlConnection(_connStr))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT * FROM tblRankFollowers WHERE broadcaster = @broadcaster", conn))
                {
                    cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = broadcasterId;
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                if (chatter.Equals(reader["username"].ToString()))
                                {
                                    return int.Parse(reader["exp"].ToString());
                                }
                            }
                        }
                    }
                }
            }

            return -1;
        }

        public void UpdateExp(string chatter, int broadcasterId, int currExp)
        {
            // Give follower experience for watching
            string query = "UPDATE tblRankFollowers SET exp = @exp WHERE (username = @username AND broadcaster = @broadcaster)";

            using (SqlConnection conn = new SqlConnection(_connStr))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.Add("@exp", SqlDbType.Int).Value = ++currExp; // add 1 experience every iteration
                cmd.Parameters.Add("@username", SqlDbType.VarChar, 30).Value = chatter;
                cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = broadcasterId;

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void EnlistRecruit(string chatter, int broadcasterId)
        {
            // Add new follower to the ranks
            string query = "INSERT INTO tblRankFollowers (username, exp, broadcaster) "
                + "VALUES (@username, @exp, @broadcaster)";

            using (SqlConnection conn = new SqlConnection(_connStr))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.Add("@username", SqlDbType.VarChar, 30).Value = chatter;
                cmd.Parameters.Add("@exp", SqlDbType.Int).Value = 0; // initial experience
                cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = broadcasterId;

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public List<Rank> GetRankList(int broadcasterId)
        {
            List<Rank> ranksList = new List<Rank>();

            // Get list of ranks currently for the specific broadcaster
            using (SqlConnection conn = new SqlConnection(_connStr))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT * FROM tblRank WHERE broadcaster = @broadcaster", conn))
                {
                    cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = broadcasterId;
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                Rank rank = new Rank()
                                {
                                    Name = reader["name"].ToString(),
                                    ExpCap = int.Parse(reader["expCap"].ToString())
                                };
                                ranksList.Add(rank);
                            }
                        }
                    }
                }
            }

            return ranksList;
        }
    }
}

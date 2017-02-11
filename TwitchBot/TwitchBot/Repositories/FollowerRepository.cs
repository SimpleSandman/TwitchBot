using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchBot.Repositories
{
    public class FollowerRepository
    {
        private string _connStr;

        public FollowerRepository(string connStr)
        {
            _connStr = connStr;
        }

        public int CurrExp(string chatter, int intBroadcasterID)
        {
            using (SqlConnection conn = new SqlConnection(_connStr))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT * FROM tblRankFollowers WHERE broadcaster = @broadcaster", conn))
                {
                    cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = intBroadcasterID;
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

        public void UpdateExp(string strChatter, int intBroadcasterID, int intCurrExp)
        {
            // Give follower experience for watching
            string query = "UPDATE tblRankFollowers SET exp = @exp WHERE (username = @username AND broadcaster = @broadcaster)";

            using (SqlConnection conn = new SqlConnection(_connStr))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.Add("@exp", SqlDbType.Int).Value = ++intCurrExp; // add 1 experience every iteration
                cmd.Parameters.Add("@username", SqlDbType.VarChar, 30).Value = strChatter;
                cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = intBroadcasterID;

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void EnlistRecruit(string strChatter, int intBroadcasterID)
        {
            // Add new follower to the ranks
            string query = "INSERT INTO tblRankFollowers (username, exp, broadcaster) "
                + "VALUES (@username, @exp, @broadcaster)";

            using (SqlConnection conn = new SqlConnection(_connStr))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.Add("@username", SqlDbType.VarChar, 30).Value = strChatter;
                cmd.Parameters.Add("@exp", SqlDbType.Int).Value = 0; // initial experience
                cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = intBroadcasterID;

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
}

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
    }
}

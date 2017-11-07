using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchBot.Repositories
{
    public class PartyUpRepository
    {
        private string _connStr;

        public PartyUpRepository(string connStr)
        {
            _connStr = connStr;
        }

        public bool HasPartyMemberBeenRequested(string username, int gameId, int broadcasterId)
        {
            bool isDuplicateRequestor = false;

            using (SqlConnection conn = new SqlConnection(_connStr))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT * FROM PartyUpRequests "
                    + "WHERE broadcaster = @broadcaster AND game = @game AND username = @username", conn))
                {
                    cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = broadcasterId;
                    cmd.Parameters.Add("@game", SqlDbType.Int).Value = gameId;
                    cmd.Parameters.Add("@username", SqlDbType.VarChar, 30).Value = username;
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                if (username.ToLower().Equals(reader["username"].ToString().ToLower()))
                                {
                                    isDuplicateRequestor = true;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            return isDuplicateRequestor;
        }

        public bool HasRequestedPartyMember(string partyMember, int gameId, int broadcasterId)
        {
            bool isPartyMemberFound = false;

            using (SqlConnection conn = new SqlConnection(_connStr))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT * FROM PartyUp WHERE broadcaster = @broadcaster AND game = @game", conn))
                {
                    cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = broadcasterId;
                    cmd.Parameters.Add("@game", SqlDbType.Int).Value = gameId;
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                if (partyMember.ToLower().Equals(reader["partyMember"].ToString().ToLower()))
                                {
                                    partyMember = reader["partyMember"].ToString();
                                    isPartyMemberFound = true;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            return isPartyMemberFound;
        }

        public void AddPartyMember(string username, string partyMember, int gameId, int broadcasterId)
        {
            string query = "INSERT INTO PartyUpRequests (username, partyMember, timeRequested, broadcaster, game) "
                                + "VALUES (@username, @partyMember, @timeRequested, @broadcaster, @game)";

            using (SqlConnection conn = new SqlConnection(_connStr))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.Add("@username", SqlDbType.VarChar, 50).Value = username;
                cmd.Parameters.Add("@partyMember", SqlDbType.VarChar, 50).Value = partyMember;
                cmd.Parameters.Add("@timeRequested", SqlDbType.DateTime).Value = DateTime.Now;
                cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = broadcasterId;
                cmd.Parameters.Add("@game", SqlDbType.Int).Value = gameId;

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public string GetPartyList(int gameId, int broadcasterId)
        {
            string partyList = "The available party members are: ";

            using (SqlConnection conn = new SqlConnection(_connStr))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT partyMember FROM PartyUp WHERE game = @game AND broadcaster = @broadcaster ORDER BY partyMember", conn))
                {
                    cmd.Parameters.Add("@game", SqlDbType.Int).Value = gameId;
                    cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = broadcasterId;
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                partyList += reader["partyMember"].ToString() + " >< ";
                            }

                            StringBuilder modPartyListMsg = new StringBuilder(partyList);
                            modPartyListMsg.Remove(partyList.Length - 4, 4); // remove extra " >< "
                            partyList = modPartyListMsg.ToString(); // replace old party member list string with new
                        }
                        else
                        {
                            partyList = "No party members are set for this game";
                        }
                    }
                }
            }

            return partyList;
        }

        public string GetRequestList(int gameId, int broadcasterId)
        {
            string partyRequestList = "Here are the requested party members: ";

            using (SqlConnection conn = new SqlConnection(_connStr))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT username, partyMember FROM PartyUpRequests "
                    + "WHERE game = @game AND broadcaster = @broadcaster ORDER BY Id", conn))
                {
                    cmd.Parameters.Add("@game", SqlDbType.Int).Value = gameId;
                    cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = broadcasterId;
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                partyRequestList += reader["partyMember"].ToString() + " <-- @" + reader["username"].ToString() + " // ";
                            }

                            StringBuilder modPartyListMsg = new StringBuilder(partyRequestList);
                            modPartyListMsg.Remove(partyRequestList.Length - 4, 4); // remove extra " // "
                            partyRequestList = modPartyListMsg.ToString(); // replace old party member list string with new
                        }
                        else
                        {
                            partyRequestList = "No party members have been requested for this game";
                        }
                    }
                }
            }

            return partyRequestList;
        }

        public string FirstRequestedPartyMember(int broadcasterId)
        {
            string removedPartyMember = "";

            using (SqlConnection conn = new SqlConnection(_connStr))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT TOP(1) partyMember, username FROM PartyUpRequests WHERE broadcaster = @broadcaster ORDER BY id", conn))
                {
                    cmd.Parameters.AddWithValue("@broadcaster", broadcasterId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                removedPartyMember = reader["partyMember"].ToString();
                                break;
                            }
                        }
                    }
                }
            }

            return removedPartyMember;
        }

        public void PopRequestedPartyMember(int broadcasterId)
        {
            string query = "WITH T AS (SELECT TOP(1) * FROM PartyUpRequests WHERE broadcaster = @broadcaster ORDER BY id) DELETE FROM T";

            using (SqlConnection conn = new SqlConnection(_connStr))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = broadcasterId;

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
}

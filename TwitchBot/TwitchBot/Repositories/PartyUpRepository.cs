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
                    + "WHERE Broadcaster = @broadcaster AND Game = @game AND Username = @username", conn))
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
                                if (username.ToLower().Equals(reader["Username"].ToString().ToLower()))
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
                using (SqlCommand cmd = new SqlCommand("SELECT * FROM PartyUp WHERE Broadcaster = @broadcaster AND Game = @game", conn))
                {
                    cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = broadcasterId;
                    cmd.Parameters.Add("@game", SqlDbType.Int).Value = gameId;
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                if (partyMember.ToLower().Equals(reader["PartyMember"].ToString().ToLower()))
                                {
                                    partyMember = reader["PartyMember"].ToString();
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
            string query = "INSERT INTO PartyUpRequests (Username, PartyMember, TimeRequested, Broadcaster, Game) "
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
                using (SqlCommand cmd = new SqlCommand("SELECT PartyMember FROM PartyUp WHERE Game = @game AND Broadcaster = @broadcaster ORDER BY PartyMember", conn))
                {
                    cmd.Parameters.Add("@game", SqlDbType.Int).Value = gameId;
                    cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = broadcasterId;
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                partyList += reader["PartyMember"].ToString() + " >< ";
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
                using (SqlCommand cmd = new SqlCommand("SELECT Username, PartyMember FROM PartyUpRequests "
                    + "WHERE Game = @game AND Broadcaster = @broadcaster ORDER BY Id", conn))
                {
                    cmd.Parameters.Add("@game", SqlDbType.Int).Value = gameId;
                    cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = broadcasterId;
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                partyRequestList += reader["PartyMember"].ToString() + " <-- @" + reader["Username"].ToString() + " // ";
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
                using (SqlCommand cmd = new SqlCommand("SELECT TOP(1) PartyMember, Username FROM PartyUpRequests WHERE Broadcaster = @broadcaster ORDER BY Id", conn))
                {
                    cmd.Parameters.AddWithValue("@broadcaster", broadcasterId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                removedPartyMember = reader["PartyMember"].ToString();
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
            string query = "WITH T AS (SELECT TOP(1) * FROM PartyUpRequests WHERE Broadcaster = @broadcaster ORDER BY Id) DELETE FROM T";

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

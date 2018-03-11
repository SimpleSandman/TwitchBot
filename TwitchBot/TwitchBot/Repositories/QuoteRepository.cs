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
    public class QuoteRepository
    {
        private string _connStr;

        public QuoteRepository(string connStr)
        {
            _connStr = connStr;
        }

        public List<Quote> GetQuotes(int broadcasterId)
        {
            List<Quote> quotes = new List<Quote>();

            using (SqlConnection conn = new SqlConnection(_connStr))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT * FROM Quote WHERE Broadcaster = @broadcaster", conn))
                {
                    cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = broadcasterId;
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                Quote quote = new Quote
                                {
                                    Message = reader["UserQuote"].ToString(),
                                    Author = reader["Username"].ToString(),
                                    TimeCreated = Convert.ToDateTime(reader["TimeCreated"])
                                };
                                quotes.Add(quote);
                            }
                        }
                    }
                }
            }

            return quotes;
        }

        public void AddQuote(string quote, string username, int broadcasterId)
        {
            string query = "INSERT INTO Quote (UserQuote, Username, TimeCreated, Broadcaster) VALUES (@userQuote, @username, @timeCreated, @broadcaster)";

            using (SqlConnection conn = new SqlConnection(_connStr))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.Add("@userQuote", SqlDbType.VarChar, 500).Value = quote;
                cmd.Parameters.Add("@username", SqlDbType.VarChar, 30).Value = username;
                cmd.Parameters.Add("@timeCreated", SqlDbType.DateTime).Value = DateTime.Now;
                cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = broadcasterId;

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
}

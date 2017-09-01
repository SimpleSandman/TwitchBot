using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchBot.Repositories
{
    public class ManualSongRequestRepository
    {
        private string _connStr;

        public ManualSongRequestRepository(string connStr)
        {
            _connStr = connStr;
        }

        public void AddSongRequest(string songRequestName, string username, int broadcasterId)
        {
            string query = "INSERT INTO tblSongRequests (songRequests, broadcaster, chatter) VALUES (@song, @broadcaster, @chatter)";

            using (SqlConnection conn = new SqlConnection(_connStr))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.Add("@song", SqlDbType.VarChar, 200).Value = songRequestName;
                cmd.Parameters.Add("@broadcaster", SqlDbType.VarChar, 200).Value = broadcasterId;
                cmd.Parameters.Add("@chatter", SqlDbType.VarChar, 200).Value = username;

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public string ListSongRequests(int broadcasterId)
        {
            string songList = "";

            using (SqlConnection conn = new SqlConnection(_connStr))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT songRequests FROM tblSongRequests WHERE broadcaster = @broadcaster", conn))
                {
                    cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = broadcasterId;
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            DataTable schemaTable = reader.GetSchemaTable();
                            DataTable data = new DataTable();
                            foreach (DataRow row in schemaTable.Rows)
                            {
                                string colName = row.Field<string>("ColumnName");
                                Type t = row.Field<Type>("DataType");
                                data.Columns.Add(colName, t);
                            }
                            while (reader.Read())
                            {
                                var newRow = data.Rows.Add();
                                foreach (DataColumn col in data.Columns)
                                {
                                    newRow[col.ColumnName] = reader[col.ColumnName];
                                    Console.WriteLine(newRow[col.ColumnName]);
                                    songList = songList + newRow[col.ColumnName] + " >< ";
                                }
                            }
                            StringBuilder strBdrSongList = new StringBuilder(songList);
                            strBdrSongList.Remove(songList.Length - 4, 4); // remove extra " >< "
                            songList = strBdrSongList.ToString(); // replace old song list string with new
                        }
                    }
                }
            }

            return songList;
        }

        public string GetFirstSongRequest(int broadcasterId)
        {
            string firstSong = "";

            using (SqlConnection conn = new SqlConnection(_connStr))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT TOP(1) songRequests FROM tblSongRequests WHERE broadcaster = @broadcaster ORDER BY id", conn))
                {
                    cmd.Parameters.AddWithValue("@broadcaster", broadcasterId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                firstSong = reader["songRequests"].ToString();
                                break;
                            }
                        }
                    }
                }
            }

            return firstSong;
        }

        public void PopSongRequest(int broadcasterId)
        {
            string query = "WITH T AS (SELECT TOP(1) * FROM tblSongRequests WHERE broadcaster = @broadcaster ORDER BY id) DELETE FROM T";

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

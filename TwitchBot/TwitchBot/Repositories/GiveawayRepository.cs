using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchBot.Repositories
{
    public class GiveawayRepository
    {
        private string _connStr;

        public GiveawayRepository(string connStr)
        {
            _connStr = connStr;
        }

        public void AddGiveaway(DateTime giveawayDate, string giveawayText, int broadcasterId, int[] elgList, 
            int giveawayType, string giveawayParam, string minRandNum, string maxRandNum)
        {
            string query = "INSERT INTO Giveaway (dueDate, message, broadcaster, elgMod, elgReg, elgSub, " 
                               + "elgUsr, giveType, giveParam1, giveParam2) " +
                           "VALUES (@dueDate, @message, @broadcaster, @elgMod, @elgReg, @elgSub, " 
                               + "@elgUsr, @giveType, @giveParam1, @giveParam2)";

            using (SqlConnection conn = new SqlConnection(_connStr))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.Add("@dueDate", SqlDbType.DateTime).Value = giveawayDate;
                cmd.Parameters.Add("@message", SqlDbType.VarChar, 75).Value = giveawayText;
                cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = broadcasterId;
                cmd.Parameters.Add("@elgMod", SqlDbType.Bit).Value = elgList[0];
                cmd.Parameters.Add("@elgReg", SqlDbType.Bit).Value = elgList[1];
                cmd.Parameters.Add("@elgSub", SqlDbType.Bit).Value = elgList[2];
                cmd.Parameters.Add("@elgUsr", SqlDbType.Bit).Value = elgList[3];
                cmd.Parameters.Add("@giveType", SqlDbType.Int).Value = giveawayType;

                if (giveawayType == 1) // keyword
                {
                    cmd.Parameters.Add("@giveParam1", SqlDbType.VarChar, 50).Value = giveawayParam;
                    cmd.Parameters.Add("@giveParam2", SqlDbType.VarChar, 50).Value = DBNull.Value;
                }
                else if (giveawayType == 2) // random number
                {
                    cmd.Parameters.Add("@giveParam1", SqlDbType.VarChar, 50).Value = minRandNum;
                    cmd.Parameters.Add("@giveParam2", SqlDbType.VarChar, 50).Value = maxRandNum;
                }

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public int GetGiveawayId(int reqGiveawayId, int broadcasterId)
        {
            int giveawayId = -1;

            using (SqlConnection conn = new SqlConnection(_connStr))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT id, broadcaster FROM Giveaway "
                    + "WHERE broadcaster = @broadcaster", conn))
                {
                    cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = broadcasterId;
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                if (reqGiveawayId.ToString().Equals(reader["id"].ToString()))
                                {
                                    giveawayId = int.Parse(reader["id"].ToString());
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            return giveawayId;
        }

        public void UpdateGiveaway(int inputType, DateTime giveawayDate, string giveawayInput, int[] elgList, 
            int giveawayType, int giveawayId, int broadcasterId, string giveawayTypeParam1, string giveawayTypeParam2)
        {
            string query = "";

            if (inputType == 1)
                query = "UPDATE dbo.Giveaway SET dueDate = @dueDate WHERE (Id = @id AND broadcaster = @broadcaster)";
            else if (inputType == 2)
                query = "UPDATE dbo.Giveaway SET message = @message WHERE (Id = @id AND broadcaster = @broadcaster)";
            else if (inputType == 3)
            {
                query = "UPDATE dbo.Giveaway SET elgMod = @elgMod" +
                    ", elgReg = @elgReg" +
                    ", elgSub = @elgSub" +
                    ", elgUsr = @elgUsr" +
                    " WHERE (Id = @id AND broadcaster = @broadcaster)";
            }
            else if (inputType == 4)
            {
                query = "UPDATE dbo.Giveaway SET giveType = @giveType" +
                    ", giveParam1 = @giveParam1" +
                    ", giveParam2 = @giveParam2" +
                    " WHERE (Id = @id AND broadcaster = @broadcaster)";
            }

            using (SqlConnection conn = new SqlConnection(_connStr))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                // append proper parameter(s)
                if (inputType == 1)
                    cmd.Parameters.Add("@dueDate", SqlDbType.DateTime).Value = giveawayDate;
                else if (inputType == 2)
                    cmd.Parameters.Add("@message", SqlDbType.VarChar, 50).Value = giveawayInput;
                else if (inputType == 3)
                {
                    cmd.Parameters.Add("@elgMod", SqlDbType.Bit).Value = elgList[0];
                    cmd.Parameters.Add("@elgReg", SqlDbType.Bit).Value = elgList[1];
                    cmd.Parameters.Add("@elgSub", SqlDbType.Bit).Value = elgList[2];
                    cmd.Parameters.Add("@elgUsr", SqlDbType.Bit).Value = elgList[3];
                }
                else if (inputType == 4)
                {
                    cmd.Parameters.Add("@giveType", SqlDbType.Int).Value = giveawayType;
                    cmd.Parameters.Add("@giveParam1", SqlDbType.VarChar, 50).Value = giveawayTypeParam1;

                    if (giveawayType == 2)
                        cmd.Parameters.Add("@giveParam2", SqlDbType.VarChar, 50).Value = giveawayTypeParam2;
                    else
                        cmd.Parameters.Add("@giveParam2", SqlDbType.VarChar, 50).Value = DBNull.Value;
                }

                cmd.Parameters.Add("@id", SqlDbType.Int).Value = giveawayId;
                cmd.Parameters.Add("@broadcaster", SqlDbType.Int).Value = broadcasterId;

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
}

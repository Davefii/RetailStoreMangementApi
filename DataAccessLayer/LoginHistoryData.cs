using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer
{
    public class LoginHistoryDTO
    {
        public int ID { get; set; }
        public int UserID { get; set; }
        public string UserName { get; set; }
        public DateTime LoginTime { get; set; }

        public LoginHistoryDTO()
        {
            ID = -1;
            UserID = -1;
            UserName = string.Empty;
            LoginTime = DateTime.Now;
        }
        public LoginHistoryDTO(int id, int userId, string userName, DateTime loginTime)
        {
            ID = id;
            UserID = userId;
            UserName = userName;
            LoginTime = loginTime;
        }
    }
    public class LoginHistoryData
    {
        public static List<LoginHistoryDTO> GetAllLoginHistory()
        {
            List<LoginHistoryDTO> loginHistoryList = new List<LoginHistoryDTO>();
            using (SqlConnection connection = new SqlConnection(DataAccessSettings.DataAccessSettings.ConnictionString))
            {
                using (SqlCommand cmd = new SqlCommand("GetAllLoginhistory", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    try
                    {
                        connection.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                loginHistoryList.Add(new LoginHistoryDTO
                                (
                                    reader.GetInt32(reader.GetOrdinal("ID")),
                                    reader.GetInt32(reader.GetOrdinal("UserID")),
                                    reader.GetString(reader.GetOrdinal("UserName")),
                                    reader.GetDateTime(reader.GetOrdinal("Datelog"))
                                ));
                            }
                        }
                    }
                    catch (Exception ex) { Debug.WriteLine(ex); }
                    finally { if (connection.State == ConnectionState.Open) connection.Close(); }
                }
                return loginHistoryList;
            }
        }
    }
}

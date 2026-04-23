using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer
{
    public class UserDTO
    {
        public int ID { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public byte Permition { get; set; }
        public bool IsActive { get; set; }

        public UserDTO()
        {
            this.ID = -1;
            this.UserName = string.Empty;
            this.Password = string.Empty;
            this.Permition = 0;
            this.IsActive = false;
        }

        public UserDTO(int id, string username, string password, byte permition, bool isActive)
        {
            this.ID = id;
            this.UserName = username;
            this.Password = password;
            this.Permition = permition;
            this.IsActive = isActive;
        }
    }

    public class DataUsers
    {
        public static List<UserDTO> GetAllUsers()
        {
            List<UserDTO> UserList = new List<UserDTO>();
            using (SqlConnection connection = new SqlConnection(DataAccessSettings.DataAccessSettings.ConnictionString))
            {
                using (SqlCommand cmd = new SqlCommand("GetallUsersforApi", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    try
                    {
                        connection.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                UserList.Add(new UserDTO
                                (
                                    reader.GetInt32(reader.GetOrdinal("ID")),
                                    reader.GetString(reader.GetOrdinal("UserName")),
                                    reader.GetString(reader.GetOrdinal("Password")),
                                    reader.GetByte(reader.GetOrdinal("Permissions")),
                                    reader.GetBoolean(reader.GetOrdinal("isActive"))
                                ));
                            }
                        }
                    }
                    catch (Exception ex) { Debug.WriteLine(ex); }
                    finally { if (connection.State == ConnectionState.Open) connection.Close(); }
                }
            }
            return UserList;
        }
        public static bool GetUserByID(int ID, ref UserDTO userDTO)
        {
            bool isFound = false;
            using (SqlConnection connection = new SqlConnection(DataAccessSettings.DataAccessSettings.ConnictionString))
            using (SqlCommand command = new SqlCommand("SP_GetUserByID", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@ID", ID);
                var UserNameParam = new SqlParameter("@UserName", SqlDbType.VarChar, 50) { Direction = ParameterDirection.Output };
                var PasswordParam = new SqlParameter("@Password", SqlDbType.VarChar, 50) { Direction = ParameterDirection.Output };
                var PermitionParam = new SqlParameter("@Permissions", SqlDbType.TinyInt) { Direction = ParameterDirection.Output };
                var IsActiveParam = new SqlParameter("@isActive", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                var IsFoundParam = new SqlParameter("@IsFound", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                command.Parameters.Add(UserNameParam);
                command.Parameters.Add(PasswordParam);
                command.Parameters.Add(PermitionParam);
                command.Parameters.Add(IsActiveParam);
                command.Parameters.Add(IsFoundParam);
                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    userDTO.ID = ID;
                    if (UserNameParam.Value != DBNull.Value) userDTO.UserName = UserNameParam.Value.ToString();
                    if (PasswordParam.Value != DBNull.Value) userDTO.Password = PasswordParam.Value.ToString();
                    if (PermitionParam.Value != DBNull.Value) userDTO.Permition = Convert.ToByte(PermitionParam.Value);
                    if (IsActiveParam.Value != DBNull.Value) userDTO.IsActive = Convert.ToBoolean(IsActiveParam.Value);
                    isFound = (IsFoundParam.Value != DBNull.Value) && Convert.ToBoolean(IsFoundParam.Value);
                }
                catch (Exception ex) { Debug.WriteLine(ex); isFound = false; }
                finally { if (connection.State == ConnectionState.Open) connection.Close(); }
                return isFound;
            }
        }
        public static bool GetUserByUserName(string username, ref UserDTO userDTO)
        {
            bool isFound = false;
            string query = @"
        SELECT ID, UserName, Password, Permissions, IsActive
        FROM Users
        WHERE UserName = @UserName";
            using (SqlConnection connection = new SqlConnection(DataAccessSettings.DataAccessSettings.ConnictionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@UserName", username);
                try
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            isFound = true;
                            userDTO.ID = reader["ID"] != DBNull.Value ? (int)reader["ID"] : 0;
                            userDTO.UserName = reader["UserName"]?.ToString();
                            userDTO.Password = reader["Password"]?.ToString();
                            userDTO.Permition = reader["Permissions"] != DBNull.Value ? Convert.ToByte(reader["Permissions"]) : (byte)0;
                            userDTO.IsActive = reader["IsActive"] != DBNull.Value && Convert.ToBoolean(reader["IsActive"]);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    return false;
                }
            }
            return isFound;
        }
        //public static bool GetUserByUserNameandPassword(string username, string password, ref UserDTO userDTO)
        //{
        //    bool isFound = false;
        //    using (SqlConnection connection = new SqlConnection(DataAccessSettings.DataAccessSettings.ConnictionString))
        //    using (SqlCommand command = new SqlCommand("Sp_GetUserByUserNameAndPassword", connection))
        //    {
        //        command.CommandType = CommandType.StoredProcedure;
        //        command.Parameters.AddWithValue("@UserName", username);
        //        command.Parameters.AddWithValue("@Password", password);
        //        var IDParam = new SqlParameter("@ID", SqlDbType.Int) { Direction = ParameterDirection.Output };
        //        var PasswordHashParam = new SqlParameter("@Password", SqlDbType.VarChar, 50) { Direction = ParameterDirection.Output };
        //        var PermitionParam = new SqlParameter("@Permition", SqlDbType.TinyInt) { Direction = ParameterDirection.Output };
        //        var IsActiveParam = new SqlParameter("@isActive", SqlDbType.Bit) { Direction = ParameterDirection.Output };
        //        var IsFoundParam = new SqlParameter("@IsFound", SqlDbType.Bit) { Direction = ParameterDirection.Output };
        //        command.Parameters.Add(IDParam);
        //        command.Parameters.Add(PasswordHashParam);
        //        command.Parameters.Add(PermitionParam);
        //        command.Parameters.Add(IsActiveParam);
        //        command.Parameters.Add(IsFoundParam);
        //        try
        //        {
        //            connection.Open();
        //            command.ExecuteNonQuery();
        //            if (IDParam.Value != DBNull.Value) userDTO.ID = (int)IDParam.Value;
        //            userDTO.UserName = username;
        //            if (PasswordHashParam.Value != DBNull.Value) userDTO.Password = PasswordHashParam.Value.ToString();
        //            if (IsActiveParam.Value != DBNull.Value) userDTO.IsActive = Convert.ToBoolean(IsActiveParam.Value);
        //            if (PermitionParam.Value != DBNull.Value) userDTO.Permition = Convert.ToByte(PermitionParam.Value);
        //            isFound = (IsFoundParam.Value != DBNull.Value) && Convert.ToBoolean(IsFoundParam.Value);
        //        }
        //        catch (Exception ex) { Debug.WriteLine(ex); isFound = false; }
        //        finally { if (connection.State == ConnectionState.Open) connection.Close(); }
        //        return isFound;
        //    }
        //}
        public static int AddNewUser(UserDTO userDTO)
        {
            int? UserID = null;
            using (SqlConnection connection = new SqlConnection(DataAccessSettings.DataAccessSettings.ConnictionString))
            using (SqlCommand command = new SqlCommand("Sp_AddNewUser", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@UserName", userDTO.UserName);
                command.Parameters.AddWithValue("@Password", userDTO.Password);
                command.Parameters.AddWithValue("@Permissions", userDTO.Permition);
                command.Parameters.AddWithValue("@isActive", userDTO.IsActive);
                SqlParameter outputparameter = new SqlParameter("@NewID", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(outputparameter);
                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    UserID = (int)command.Parameters["@NewID"].Value;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
                finally
                {
                    if (connection.State == ConnectionState.Open)
                    {
                        connection.Close();
                    }
                }
            }
            return UserID ?? -1;
        }
        public static bool UpdateUser(UserDTO userDTO)
        {
            int RowAffected = 0;
            using (SqlConnection connection = new SqlConnection(DataAccessSettings.DataAccessSettings.ConnictionString))
            using (SqlCommand command = new SqlCommand("Sp_UpdateUser", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@ID", userDTO.ID);
                command.Parameters.AddWithValue("@UserName", userDTO.UserName);
                command.Parameters.AddWithValue("@Password", userDTO.Password);
                command.Parameters.AddWithValue("@Permissions", userDTO.Permition);
                command.Parameters.AddWithValue("@isActive", userDTO.IsActive);
                SqlParameter returnparameter = new SqlParameter();
                returnparameter.Direction = ParameterDirection.ReturnValue;
                command.Parameters.Add(returnparameter);
                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    RowAffected = (int)returnparameter.Value;
                }
                catch (Exception ex) { Debug.WriteLine(ex); }
                finally { if (connection.State == ConnectionState.Open) connection.Close(); }
                return RowAffected > 0;
            }
        }
        public static bool DeleteUser(int ID)
        {
            int RowAffected = 0;
            using (SqlConnection connection = new SqlConnection(DataAccessSettings.DataAccessSettings.ConnictionString))
            using (SqlCommand command = new SqlCommand("Sp_DeleteUser", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@UserID", ID);
                SqlParameter returnparameter = new SqlParameter();
                returnparameter.Direction = ParameterDirection.ReturnValue;
                command.Parameters.Add(returnparameter);
                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    RowAffected = (int)returnparameter.Value;
                }
                catch (Exception ex) { Debug.WriteLine(ex); }
                finally { if (connection.State == ConnectionState.Open) connection.Close(); }
                return RowAffected > 0;
            }
        }
        public static bool ChangePassword(int ID, string password)
        {
            int RowAffected = 0;
            using (SqlConnection connection = new SqlConnection(DataAccessSettings.DataAccessSettings.ConnictionString))
            using (SqlCommand command = new SqlCommand("Sp_ChangePassword", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@ID", ID);
                command.Parameters.AddWithValue("@Password", password);
                SqlParameter returnparameter = new SqlParameter();
                returnparameter.Direction = ParameterDirection.ReturnValue;
                command.Parameters.Add(returnparameter);
                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    RowAffected = (int)returnparameter.Value;
                }
                catch (Exception ex) { Debug.WriteLine(ex); }
                finally { if (connection.State == ConnectionState.Open) connection.Close(); }
                return RowAffected > 0;
            }
        }
        public static byte AddLogos(int User_ID, DateTime Datelog)
        {
            byte? LogsID = null;
            using (SqlConnection connection = new SqlConnection(DataAccessSettings.DataAccessSettings.ConnictionString))
            using (SqlCommand command = new SqlCommand("Sp_AddNewLogs", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@UserID", User_ID);
                command.Parameters.AddWithValue("@DateLog", Datelog);
                SqlParameter outputparameter = new SqlParameter("@NewID", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(outputparameter);
                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    LogsID = (byte)command.Parameters["@NewID"].Value;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
                finally
                {
                    if (connection.State == ConnectionState.Open)
                    {
                        connection.Close();
                    }
                }
            }
            return LogsID ?? 0;
        }
    }
}

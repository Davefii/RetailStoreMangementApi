using BCrypt.Net;
using DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BussinessLayer
{
    public class Users
    {
        enum enMode { Addnew = 0, Update = 1 }
        [Flags]
        public enum enMainMenuPermitions
        { 
            Viewer = 1 ,
            StaffOrCashier = 2,
            Admin = 4
        }
        public int ID { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public enMainMenuPermitions Permitions { get; set; }
        public bool isActive { get; set; }
        public string RefreshTokenHash { get; set; }
        public DateTime? RefreshTokenExpiresAt { get; set; }
        public DateTime? RefreshTokenRevokedAt { get; set; }
        public UserDTO userDTO { get { return new UserDTO() { ID = this.ID, UserName = this.UserName, Password = this.Password, Permition = (byte)this.Permitions, IsActive = this.isActive }; } }
        enMode Mode;
        public Users ()
        {
            this.ID = -1;
            this.UserName = string.Empty;
            this.Password = string.Empty;
            this.Permitions = 0;
            this.isActive = false;
            Mode = enMode.Addnew;
        }
        public Users(UserDTO userDTO)
        {
            this.ID = userDTO.ID;
            this.UserName = userDTO.UserName;
            this.Password = userDTO.Password;
            this.Permitions = (enMainMenuPermitions)userDTO.Permition;
            this.isActive = userDTO.IsActive;
            Mode = enMode.Addnew;
        }
        private Users(UserDTO userDTO, enMode mode = enMode.Addnew)
        {
            this.ID = userDTO.ID;
            this.UserName = userDTO.UserName;
            this.Password = userDTO.Password;
            this.Permitions = (enMainMenuPermitions)userDTO.Permition;
            this.isActive = userDTO.IsActive;
            Mode = enMode.Update;
        }
        public static List<UserDTO> GetAllUsers()
        {
            return DataUsers.GetAllUsers();
        }
        public static List<LoginHistoryDTO> GetHistoryUserlogin()
        {
            return LoginHistoryData.GetAllLoginHistory();
        }

        public static Users GetUserByID(int ID)
        {
            UserDTO userDTO = new UserDTO();
            bool isFound = DataUsers.GetUserByID(ID, ref userDTO);
            if (isFound)
                return new Users(userDTO, enMode.Update);
            else
                return null;
        }
        public static Users GetUserName(string UserName, string Password)
        {
            UserDTO userDTO = new UserDTO();

            bool isFound = DataUsers.GetUserByUserName(UserName, ref userDTO);
            // Step 1: Ensure user exists
            if (!isFound)
                return null;
            //// Step 2: Validate stored hash
            //if (string.IsNullOrEmpty(userDTO.Password))
            //    return null;
            //// Optional: detect corrupted hash
            //if (!userDTO.Password.StartsWith("$2") || userDTO.Password.Length < 60)
            //    return null;
            // Step 3: Verify password
            bool verify = BCrypt.Net.BCrypt.Verify(Password, userDTO.Password);
            if (!verify)
                return null;

            byte savehistory = DataUsers.AddLogos(userDTO.ID, DateTime.Now);
            if (isFound && savehistory >= 0)
                return new Users(userDTO, enMode.Update);
            else
                return null;
        }
        public static Users GetUserNameforRefreshToken(string UserName)
        {
            UserDTO userDTO = new UserDTO();

            bool isFound = DataUsers.GetUserByUserName(UserName, ref userDTO);
            // Step 1: Ensure user exists
            if (!isFound)
                return null;
            if (isFound)
                return new Users(userDTO, enMode.Update);
            else
                return null;
        }
        public int Addnewuser()
        {
            string passhashed = BCrypt.Net.BCrypt.HashPassword(this.Password);
            this.Password = passhashed;
            this.ID = DataUsers.AddNewUser(userDTO);
            return this.ID;
        }
        public bool UpdateUser()
        {
            string passhashed = BCrypt.Net.BCrypt.HashPassword(this.Password);
            return DataUsers.UpdateUser(userDTO);
        }
        public bool DeleteUser()
        {
            return DataUsers.DeleteUser(this.ID);
        }
        //public bool Save()
        //{
        //    switch (this.Mode)
        //    {
        //        case enMode.Addnew:
        //            {
        //                return Addnewuser();
        //            }
        //        case enMode.Update:
        //            {
        //                return UpdateUser();
        //            }
        //        default:
        //            return false;
        //    }
        //}
        public bool Changepassword(string Password)
        {
            string passhashed = BCrypt.Net.BCrypt.HashPassword(Password);
            Debug.WriteLine(userDTO.Password.Length);
            Debug.WriteLine("[" + userDTO.Password + "]");
            return DataUsers.ChangePassword(this.ID, Password);
        }
        public static bool ChangepasswordAnyone(int ID, string Password)
        {
            //setpassword();

            string passhashed = BCrypt.Net.BCrypt.HashPassword(Password);
            return DataUsers.ChangePassword(ID, passhashed);
        }
    }
}

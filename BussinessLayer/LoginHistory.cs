using DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BussinessLayer
{
    public class LoginHistory
    {
        public int ID { get; set; }
        public int UserID { get; set; }
        public string UserName { get; set; }
        public DateTime LoginTime { get; set; }
        public LoginHistoryDTO loginHistoryDTO
        {
            get
            {
                return new LoginHistoryDTO
                {
                    ID = this.ID,
                    UserID = this.UserID,
                    UserName = this.UserName,
                    LoginTime = this.LoginTime
                };
            }
        }
        public LoginHistory()
        {
            ID = -1;
            UserID = -1;
            UserName = string.Empty;
            LoginTime = DateTime.Now;
        }
        public LoginHistory(LoginHistoryDTO loginHistoryDTO)
        {
            ID = loginHistoryDTO.ID;
            UserID = loginHistoryDTO.UserID;
            UserName = loginHistoryDTO.UserName;
            LoginTime = loginHistoryDTO.LoginTime;
        }

        public List<LoginHistoryDTO> GetHistoryLogin()
        {
            return LoginHistoryData.GetAllLoginHistory();
        }
    }
}

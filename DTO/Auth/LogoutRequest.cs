namespace RetailStroeManagmentAPI.DTO.Auth
{
    public class LogoutRequest
    {
        public string UserName { get; set; }
        public string RefreshToken { get; set; }
    }
}

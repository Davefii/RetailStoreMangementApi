using Azure.Core;
using BussinessLayer;
using DataAccessLayer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using RetailStroeManagmentAPI.DTO.Auth;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace RetailStroeManagmentAPI.Controllers
{

    public class Login : Controller
    {
        private readonly ILogger<Login> _logger;

        public Login(ILogger<Login> logger)
        {
            _logger = logger;
        }

        private static string GenerateRefreshToken()
        {
            var bytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }
        private string GetRoleFromPermission(byte permission)
        {
            return permission switch
            {
                4 => "Admin",
                2 => "StaffOrCashier",
                1 => "Viewer",
                _ => "Viewer"
            };
        }
        [HttpPost("LoginUser", Name = "LoginUser")]
        [EnableRateLimiting("AuthLimiter")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<UserDTO> LoginUser(UserDTO loginUserRequest)
        {
            if (loginUserRequest == null ||
                string.IsNullOrEmpty(loginUserRequest.UserName) ||
                string.IsNullOrEmpty(loginUserRequest.Password))
            {
                return BadRequest("Please insert UserName and Password");
            }
            
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            Users User = Users.GetUserName(loginUserRequest.UserName, loginUserRequest.Password);

            if (User == null)
            {
                _logger.LogWarning(
                    "Failed login attempt (user not found). User={User}, IP={IP}",
                    loginUserRequest.UserName,
                    ip);

                return Unauthorized("Invalid credentials");
            }



            // Step 3: Create claims that represent the authenticated user's identity.
            // These claims will be embedded inside the JWT.
            var claims = new[]
            {
                // Unique identifier for the student
                new Claim(ClaimTypes.NameIdentifier, User.UserName.ToString()),


                // Role (Student or Admin) used later for authorization
                new Claim(ClaimTypes.Role, GetRoleFromPermission((byte)User.Permitions))
            };


            // Step 4: Create the symmetric security key used to sign the JWT.
            // This key must match the key used in JWT validation middleware.
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes("THIS_IS_A_VERY_BNECRET_KEY_123456"));


            // Step 5: Define the signing credentials.
            // This specifies the algorithm used to sign the token.
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);


            // Step 6: Create the JWT token.
            // The token includes issuer, audience, claims, expiration, and signature.
            var token = new JwtSecurityToken(
                issuer: "RetailstoreApi",
                audience: "RetailstoreUsers",
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds
            );


            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

            // Create refresh token (random)
            var refreshToken = GenerateRefreshToken();

            // Store refresh token securely (hash + expiry + not revoked)
            User.RefreshTokenHash = BCrypt.Net.BCrypt.HashPassword(refreshToken);
            User.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);
            User.RefreshTokenRevokedAt = null;

            // Step 7: Return the serialized JWT token to the client.
            // The client will send this token with future requests.
            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                RefreshToken = refreshToken
            });

        }


        [HttpPost("refresh")]
        [EnableRateLimiting("AuthLimiter")]
        public IActionResult Refresh([FromBody] RefreshRequest request)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            if (request == null || string.IsNullOrEmpty(request.UserName))
                return BadRequest("UserName is required");

            Users user = Users.GetUserNameforRefreshToken(request.UserName);
            
            if (user == null)
                return Unauthorized("Invalid refresh request");

            bool refreshValid = BCrypt.Net.BCrypt.Verify(request.RefreshToken, user.RefreshTokenHash);
            if (!refreshValid)
            {
                _logger.LogWarning(
                    "Failed refresh token attempt (invalid token). User={User}, IP={IP}",
                    request.UserName,
                    ip);
                return Unauthorized("Invalid refresh token");
            }

            if (user.RefreshTokenExpiresAt == null || user.RefreshTokenExpiresAt <= DateTime.UtcNow)
            {
                _logger.LogWarning(
                    "Failed refresh token attempt (expired token). User={User}, IP={IP}",
                    request.UserName,
                    ip);
                return Unauthorized("Refresh token expired");
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserName),
                new Claim(ClaimTypes.Role, GetRoleFromPermission((byte)user.Permitions))
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes("THIS_IS_A_VERY_BNECRET_KEY_123456"));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var jwt = new JwtSecurityToken(
                issuer: "RetailstoreApi",
                audience: "RetailstoreUsers",
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: creds
            );

            var newAccessToken = new JwtSecurityTokenHandler().WriteToken(jwt);
            var newRefreshToken = GenerateRefreshToken();
            user.RefreshTokenHash = BCrypt.Net.BCrypt.HashPassword(newRefreshToken);
            user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);

            return Ok(new
            {
                token = newAccessToken,
                refreshToken = newRefreshToken
            });
        }

        [HttpPost("logout")]
        public IActionResult Logout([FromBody] LogoutRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.UserName))
                return BadRequest("UserName is required");

            Users user = Users.GetUserName(request.UserName, "");
            
            if (user == null)
                return Ok(); // Do not reveal if user exists

            bool refreshValid = BCrypt.Net.BCrypt.Verify(request.RefreshToken, user.RefreshTokenHash);
            if (!refreshValid)
                return Ok();

            // Revoke/expire the refresh token
            user.RefreshTokenRevokedAt = DateTime.UtcNow;
            return Ok("Logged out successfully");
        }
    }
}

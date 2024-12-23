using Code_Record.Server.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Code_Record.Server.Extensions.Controllers;
using Code_Record.Server.Models.Base.UserStructure;

namespace Code_Record.Server.Controllers.SQLServer.Common
{
    [Route("api/sql/common/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly SQLServerContext _context;
        private IConfiguration _config;

        public LoginController(SQLServerContext context, IConfiguration configuration)
        {
            _context = context;
            _config = configuration;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Signin([FromBody] Login login)
        {
            if (login != null)
            {
                login.AccountPass = EncryptData.EncryptPass(login.AccountPass);
                var userFound = _context.Users.FirstOrDefault(user =>
                user.Email == login.Email && user.AccountPass == login.AccountPass);

                if (userFound != null)
                {
                    var token = BuildToken(userFound);
                    await _context.SaveChangesAsync();
                    return Ok(new { token });
                }
                else return NotFound(new { error = "User not found." });
            }
            else return BadRequest();
        }

        private string BuildToken(Models.SQLServer.User user)
        {
            var secretKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_config["Auth:Key"]));
            var claims = new List<Claim> {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
				new Claim(ClaimTypes.Email, user.Email),
                new Claim("Subscription", user.Subscription.ToString()),
				new Claim(ClaimTypes.Role, user.Rol.ToString()),
				new Claim(JwtRegisteredClaimNames.Iat, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };
            var expiration = DateTime.Now.AddDays(Convert.ToDouble(_config["Auth:Expiration"]));
            var credentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(claims: claims, expires: expiration, signingCredentials: credentials);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Code_Record.Server.Extensions.Controllers;
using Code_Record.Server.Models.MongoAtlas;

namespace Code_Record.Server.Controllers.MongoAtlas.Common
{
	[Route("api/mongo/common/[controller]")]
	[ApiController]
	public class LoginController : ControllerBase
	{
		private readonly IMongoCollection<User> _usersCollection;
		private readonly IConfiguration _config;

		public LoginController(IMongoDatabase database, IConfiguration configuration)
		{
			_usersCollection = database.GetCollection<User>("Users");
			_config = configuration;
		}

		[HttpPost]
		[AllowAnonymous]
		public async Task<IActionResult> Signin([FromBody] Models.Base.UserStructure.Login login)
		{
			if (login == null)
				return BadRequest(new { error = "Invalid login data." });

			login.AccountPass = EncryptData.EncryptPass(login.AccountPass);

			var userFound = await _usersCollection
				.Find(user => user.Email == login.Email && user.AccountPass == login.AccountPass)
				.FirstOrDefaultAsync();

			if (userFound == null)
				return NotFound(new { error = "User not found." });

			var token = BuildToken(userFound);
			return Ok(new { token });
		}

		private string BuildToken(User user)
		{
			var secretKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_config["Auth:Key"]));
			var claims = new List<Claim>
			{
				new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
				new Claim(ClaimTypes.Email, user.Email),
				new Claim("Subscription", user.Subscription.ToString()),
				new Claim(ClaimTypes.Role, user.Rol.ToString()),
				new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString(), ClaimValueTypes.Integer64),
				new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
			};

			var expiration = DateTime.UtcNow.AddDays(Convert.ToDouble(_config["Auth:Expiration"]));
			var credentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

			var token = new JwtSecurityToken(
				claims: claims,
				expires: expiration,
				signingCredentials: credentials);

			return new JwtSecurityTokenHandler().WriteToken(token);
		}
	}
}

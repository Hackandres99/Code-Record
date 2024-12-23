using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Security.Claims;
using Code_Record.Server.Models.MongoAtlas;
using Microsoft.AspNetCore.Authorization;
using Code_Record.Server.Extensions.Controllers;

namespace Code_Record.Server.Controllers.MongoAtlas.Common
{
	[Route("api/mongo/common/[controller]")]
	[ApiController]
	public class UsersController : ControllerBase
	{
		private readonly IMongoCollection<User> _usersCollection;

		public UsersController(IMongoDatabase database)
		{
			_usersCollection = database.GetCollection<User>("Users");
		}

		[HttpGet("me")]
		public async Task<ActionResult<User>> GetUser()
		{
			var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(id)) return Unauthorized();

			if (!ObjectId.TryParse(id, out var userId))
				return BadRequest("Invalid user ID format.");

			var user = await _usersCollection.Find(u => u.Id == userId).FirstOrDefaultAsync();
			if (user == null) return NotFound();

			return Ok(user);
		}

		[HttpPut("me")]
		public async Task<IActionResult> PutUser(User user)
		{
			var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var role = User.FindFirstValue(ClaimTypes.Role);

			if (string.IsNullOrEmpty(id) || !ObjectId.TryParse(id, out var userId) || user.Id != userId)
				return Unauthorized();

			var existingUser = await _usersCollection.Find(u => u.Id == userId).FirstOrDefaultAsync();
			if (existingUser == null) return NotFound();

			user.CreationDate = existingUser.CreationDate;
			user.UpdateDate = DateTime.UtcNow;

			if (!string.IsNullOrEmpty(user.AccountPass))
				user.AccountPass = EncryptData.EncryptPass(user.AccountPass);

			if (role != Models.Base.UserStructure.RolOptions.Admin.ToString())
			{
				user.Rol = Models.Base.UserStructure.RolOptions.Common;
				user.Subscription = Models.Base.UserStructure.SubscriptionOptions.Free;
			}

			if (string.IsNullOrEmpty(user.Username))
				return BadRequest(new { error = "Username cannot be null or empty." });

			var updateResult = await _usersCollection.ReplaceOneAsync(u => u.Id == userId, user);

			if (!updateResult.IsAcknowledged || updateResult.MatchedCount == 0)
				return NotFound();

			return NoContent();
		}

		[HttpPost]
		[AllowAnonymous]
		public async Task<ActionResult<User>> PostUser(User user)
		{
			user.Id = ObjectId.GenerateNewId();
			user.CreationDate = DateTime.UtcNow;
			user.UpdateDate = null;
			user.AccountPass = EncryptData.EncryptPass(user.AccountPass);
			user.Rol = Models.Base.UserStructure.RolOptions.Common;
			user.Subscription = Models.Base.UserStructure.SubscriptionOptions.Free;

			await _usersCollection.InsertOneAsync(user);
			return CreatedAtAction(nameof(GetUser), new { id = user.Id.ToString() }, user);
		}

		[HttpDelete("me")]
		public async Task<IActionResult> DeleteUser()
		{
			var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(id) || !ObjectId.TryParse(id, out var userId))
				return Unauthorized();

			var deleteResult = await _usersCollection.DeleteOneAsync(u => u.Id == userId);
			if (deleteResult.DeletedCount == 0)
				return NotFound();

			return NoContent();
		}
	}
}


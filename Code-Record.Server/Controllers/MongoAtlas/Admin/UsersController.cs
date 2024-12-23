using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Bson;
using Code_Record.Server.Models.MongoAtlas;
using Code_Record.Server.Extensions.Controllers;

namespace Code_Record.Server.Controllers.MongoAtlas.Admin
{
	[Route("api/mongo/admin/[controller]")]
	[ApiController]
	public class UsersController : ControllerBase
	{
		private readonly IMongoCollection<User> _usersCollection;

		public UsersController(IMongoDatabase database)
		{
			_usersCollection = database.GetCollection<User>("Users");
		}

		[HttpGet]
		public async Task<ActionResult<IEnumerable<User>>> GetUsers()
		{
			var users = await _usersCollection.Find(_ => true).ToListAsync();
			return Ok(users);
		}

		[HttpGet("{id}")]
		public async Task<ActionResult<User>> GetUser(string id)
		{
			if (!ObjectId.TryParse(id, out var objectId))
				return BadRequest(new { error = "Invalid User ID format." });

			var user = await _usersCollection.Find(u => u.Id == objectId).FirstOrDefaultAsync();
			if (user == null) return NotFound();

			return Ok(user);
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> PutUser(string id, User user)
		{
			if (!ObjectId.TryParse(id, out var objectId))
				return BadRequest(new { error = "Invalid User ID format." });

			if (id != user.Id.ToString())
				return BadRequest(new { error = "User ID does not match." });

			var existingUser = await GetExistingUser(objectId);
			if (existingUser == null) return NotFound();

			user.CreationDate = existingUser.CreationDate;
			user.UpdateDate = DateTime.UtcNow;
			user.AccountPass = EncryptData.EncryptPass(user.AccountPass);

			var result = await _usersCollection.ReplaceOneAsync(u => u.Id == objectId, user);
			if (result.MatchedCount == 0) return NotFound(new { error = "User no longer exists." });
			return NoContent();
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteUser(string id)
		{
			if (!ObjectId.TryParse(id, out var objectId))
				return BadRequest(new { error = "Invalid User ID format." });

			var result = await _usersCollection.DeleteOneAsync(u => u.Id == objectId);
			if (result.DeletedCount == 0) return NotFound(new { error = "User not found." });
			return NoContent();
		}

		private async Task<User> GetExistingUser(ObjectId id)
		{
			return await _usersCollection.Find(u => u.Id == id).FirstOrDefaultAsync();
		}
	}
}


using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Bson;
using Code_Record.Server.Models.MongoAtlas;
using Code_Record.Server.Models.MongoAtlas.SubjectStructure.TestStructure;
using System.Security.Claims;

namespace Code_Record.Server.Controllers.MongoAtlas.Common.SubjectStructure.TestStructure
{
	[Route("api/mongo/common/[controller]")]
	[ApiController]
	public class OptionsController : ControllerBase
	{
		private readonly IMongoCollection<Option> _optionsCollection;
		private readonly IMongoCollection<Allow> _allowsCollection;

		public OptionsController(IMongoDatabase database)
		{
			_optionsCollection = database.GetCollection<Option>("Options");
			_allowsCollection = database.GetCollection<Allow>("Allows");
		}

		[HttpGet("me")]
		public async Task<ActionResult<IEnumerable<Option>>> GetOptions()
		{
			var userId = GetCurrentUserId();
			if (userId == null) return Unauthorized();

			var allowedOptionIds = await _allowsCollection
				.Find(a => a.UserId == userId && a.ResourceType == "option")
				.Project(a => a.ResourceId)
				.ToListAsync();

			if (!allowedOptionIds.Any()) return Ok(new List<Option>());

			var options = await _optionsCollection
				.Find(o => allowedOptionIds.Contains(o.Id))
				.ToListAsync();

			return Ok(options);
		}

		[HttpGet("me/{id}")]
		public async Task<ActionResult<Option>> GetOption(string id)
		{
			var userId = GetCurrentUserId();
			if (userId == null) return Unauthorized();

			if (!ObjectId.TryParse(id, out var objectId))
				return BadRequest("Invalid option ID format.");

			var isAllowed = await _allowsCollection
				.Find(a => a.UserId == userId && a.ResourceType == "option" && a.ResourceId == objectId)
				.AnyAsync();

			if (!isAllowed) return Forbid();

			var option = await _optionsCollection
				.Find(o => o.Id == objectId)
				.FirstOrDefaultAsync();

			if (option == null) return NotFound();

			return Ok(option);
		}

		private ObjectId? GetCurrentUserId()
		{
			var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userIdString) || !ObjectId.TryParse(userIdString, out var userId))
				return null;

			return userId;
		}
	}
}


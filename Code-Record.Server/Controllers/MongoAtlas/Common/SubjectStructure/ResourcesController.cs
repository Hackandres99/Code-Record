using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Bson;
using Code_Record.Server.Models.MongoAtlas;
using Code_Record.Server.Models.MongoAtlas.SubjectStructure;
using System.Security.Claims;

namespace Code_Record.Server.Controllers.MongoAtlas.Common.SubjectStructure
{
	[Route("api/mongo/common/[controller]")]
	[ApiController]
	public class ResourcesController : ControllerBase
	{
		private readonly IMongoCollection<Resource> _resourcesCollection;
		private readonly IMongoCollection<Allow> _allowsCollection;

		public ResourcesController(IMongoDatabase database)
		{
			_resourcesCollection = database.GetCollection<Resource>("Resources");
			_allowsCollection = database.GetCollection<Allow>("Allows");
		}

		[HttpGet("me")]
		public async Task<ActionResult<IEnumerable<Resource>>> GetResources()
		{
			var userId = GetCurrentUserId();
			if (userId == null) return Unauthorized();

			var allowedResourceIds = await _allowsCollection
				.Find(a => a.UserId == userId && a.ResourceType == "resource")
				.Project(a => a.ResourceId)
				.ToListAsync();

			if (!allowedResourceIds.Any()) return Ok(new List<Resource>());

			var resources = await _resourcesCollection
				.Find(r => allowedResourceIds.Contains(r.Id))
				.ToListAsync();

			return Ok(resources);
		}

		[HttpGet("me/{id}")]
		public async Task<ActionResult<Resource>> GetResource(string id)
		{
			var userId = GetCurrentUserId();
			if (userId == null) return Unauthorized();

			if (!ObjectId.TryParse(id, out var objectId))
				return BadRequest("Invalid resource ID format.");

			var isAllowed = await _allowsCollection
				.Find(a => a.UserId == userId && a.ResourceType == "resource" && a.ResourceId == objectId)
				.AnyAsync();

			if (!isAllowed) return Forbid();

			var resource = await _resourcesCollection
				.Find(r => r.Id == objectId)
				.FirstOrDefaultAsync();

			if (resource == null) return NotFound();

			return Ok(resource);
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


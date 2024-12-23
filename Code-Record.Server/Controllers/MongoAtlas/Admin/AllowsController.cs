using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Bson;
using Code_Record.Server.Models.MongoAtlas;
using Code_Record.Server.Extensions.Controllers;

namespace Code_Record.Server.Controllers.MongoAtlas.Admin
{
	[Route("api/mongo/admin/[controller]")]
	[ApiController]
	public class AllowsController : ControllerBase
	{
		private readonly IMongoCollection<Allow> _allowsCollection;
		private readonly PermissionService _permissionService;

		public AllowsController(IMongoDatabase database, PermissionService permissionService)
		{
			_allowsCollection = database.GetCollection<Allow>("Allows");
			_permissionService = permissionService;
		}

		[HttpGet]
		public async Task<ActionResult<IEnumerable<Allow>>> GetAllows()
		{
			var allows = await _allowsCollection.Find(_ => true).ToListAsync();
			return Ok(allows);
		}

		[HttpGet("{id}")]
		public async Task<ActionResult<Allow>> GetAllow(string id)
		{
			if (!ObjectId.TryParse(id, out var objectId))
				return BadRequest(new { error = "Invalid ID format." });

			var allow = await _allowsCollection.Find(a => a.Id == objectId).FirstOrDefaultAsync();
			if (allow == null) return NotFound(new { error = "Allow not found." });
			return Ok(allow);
		}

		[HttpPost]
		public async Task<IActionResult> PostAllow([FromBody] Allow allow)
		{
			if (allow == null || allow.ResourceId == ObjectId.Empty || allow.UserId == ObjectId.Empty) 
				return BadRequest(new { error = "Allow fields are required and cannot be empty." });

			try
			{
				await _permissionService.AddPermissionsForSubject(allow.ResourceId, allow.UserId);
				return Ok(new { message = "Permissions successfully added." });
			}
			catch (Exception ex)
			{
				return BadRequest(new { error = ex.Message });
			}
		}

		[HttpDelete("{subjectId}/users/{userId}")]
		public async Task<IActionResult> DeleteAllow(ObjectId subjectId, ObjectId userId)
		{
			try
			{
				var deleteFilter = await _permissionService.BuildDeleteFilterAsync(subjectId, userId);
				var deleteResult = await _allowsCollection.DeleteManyAsync(deleteFilter);

				if (deleteResult.DeletedCount == 0) 
					return NotFound(new { error = "No permissions found for the given Subject and User." });

				return NoContent();
			}
			catch (Exception ex)
			{
				return BadRequest(new { error = ex.Message });
			}
		}
	}
}


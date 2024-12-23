using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Security.Claims;
using Code_Record.Server.Models.MongoAtlas;
using Code_Record.Server.Models.MongoAtlas.SubjectStructure;

namespace Code_Record.Server.Controllers.MongoAtlas.Common.SubjectStructure
{
	[Route("api/mongo/common/[controller]")]
	[ApiController]
	public class SubjectsController : ControllerBase
	{
		private readonly IMongoCollection<Subject> _subjectsCollection;
		private readonly IMongoCollection<Allow> _allowsCollection;

		public SubjectsController(IMongoDatabase database)
		{
			_subjectsCollection = database.GetCollection<Subject>("Subjects");
			_allowsCollection = database.GetCollection<Allow>("Allows");
		}

		[HttpGet("me")]
		public async Task<ActionResult<IEnumerable<Subject>>> GetSubjects()
		{
			var userId = GetCurrentUserId();
			if (userId == null) return Unauthorized();

			var allowedResourceIds = await _allowsCollection
				.Find(a => a.UserId == userId && a.ResourceType == "subject")
				.Project(a => a.ResourceId)
				.ToListAsync();

			if (!allowedResourceIds.Any()) return Ok(new List<Subject>());

			var subjects = await _subjectsCollection
				.Find(s => allowedResourceIds.Contains(s.Id))
				.ToListAsync();

			return Ok(subjects);
		}

		[HttpGet("me/{id}")]
		public async Task<ActionResult<Subject>> GetSubject(string id)
		{
			var userId = GetCurrentUserId();
			if (userId == null) return Unauthorized();

			if (!ObjectId.TryParse(id, out ObjectId subjectId))
				return BadRequest(new { error = "Invalid subject ID." });

			var isAllowed = await _allowsCollection
				.Find(a => a.UserId == userId && a.ResourceId == subjectId && a.ResourceType == "subject")
				.AnyAsync();

			if (!isAllowed) return Forbid();

			var subject = await _subjectsCollection.Find(s => s.Id == subjectId).FirstOrDefaultAsync();

			if (subject == null) return NotFound();

			return Ok(subject);
		}

		private ObjectId? GetCurrentUserId()
		{
			var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userIdString) || !ObjectId.TryParse(userIdString, out ObjectId userId))
				return null;

			return userId;
		}
	}
}


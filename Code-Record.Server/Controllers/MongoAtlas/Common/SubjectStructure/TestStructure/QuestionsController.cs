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
	public class QuestionsController : ControllerBase
	{
		private readonly IMongoCollection<Question> _questionsCollection;
		private readonly IMongoCollection<Allow> _allowsCollection;

		public QuestionsController(IMongoDatabase database)
		{
			_questionsCollection = database.GetCollection<Question>("Questions");
			_allowsCollection = database.GetCollection<Allow>("Allows");
		}

		[HttpGet("me")]
		public async Task<ActionResult<IEnumerable<Question>>> GetQuestions()
		{
			var userId = GetCurrentUserId();
			if (userId == null) return Unauthorized();

			var allowedQuestionIds = await _allowsCollection
				.Find(a => a.UserId == userId && a.ResourceType == "question")
				.Project(a => a.ResourceId)
				.ToListAsync();

			if (!allowedQuestionIds.Any()) return Ok(new List<Question>());

			var questions = await _questionsCollection
				.Find(q => allowedQuestionIds.Contains(q.Id))
				.ToListAsync();

			return Ok(questions);
		}

		[HttpGet("me/{id}")]
		public async Task<ActionResult<Question>> GetQuestion(string id)
		{
			var userId = GetCurrentUserId();
			if (userId == null) return Unauthorized();

			if (!ObjectId.TryParse(id, out var objectId))
				return BadRequest("Invalid question ID format.");

			var isAllowed = await _allowsCollection
				.Find(a => a.UserId == userId && a.ResourceType == "question" && a.ResourceId == objectId)
				.AnyAsync();

			if (!isAllowed) return Forbid();

			var question = await _questionsCollection
				.Find(q => q.Id == objectId)
				.FirstOrDefaultAsync();

			if (question == null) return NotFound();

			return Ok(question);
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


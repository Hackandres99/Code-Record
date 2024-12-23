using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Bson;
using Code_Record.Server.Models.MongoAtlas.SubjectStructure.ThemeStructure.VideoStructure.ConversationStructure;
using System.Security.Claims;

namespace Code_Record.Server.Controllers.MongoAtlas.Common.SubjectStructure.ThemeStructure.VideoStructure.ConversationStructure
{
	[Route("api/mongo/common/[controller]")]
	[ApiController]
	public class AnswersController : ControllerBase
	{
		private readonly IMongoCollection<Answer> _anwsersCollection;

		public AnswersController(IMongoDatabase database)
		{
			_anwsersCollection = database.GetCollection<Answer>("Anwsers");
		}

		[HttpGet]
		public async Task<ActionResult<IEnumerable<Answer>>> GetAnwsers()
		{
			var anwsers = await _anwsersCollection.Find(_ => true).ToListAsync();
			return Ok(anwsers);
		}

		[HttpGet("{id}")]
		public async Task<ActionResult<Answer>> GetAnwser(string id)
		{
			if (!ObjectId.TryParse(id, out var objectId))
				return BadRequest(new { error = "Invalid Answer ID format." });

			var anwser = await _anwsersCollection.Find(a => a.Id == objectId).FirstOrDefaultAsync();

			if (anwser == null) return NotFound();

			return Ok(anwser);
		}

		[HttpPut("me/{id}")]
		public async Task<IActionResult> PutAnwser(string id, Answer anwser)
		{
			var userEmail = GetCurrentUserEmail();

			if (!ObjectId.TryParse(id, out var objectId))
				return BadRequest(new { error = "Invalid Answer ID format." });

			if (anwser.Id != objectId)
				return BadRequest(new { error = "Answer ID does not match." });

			var existingAnwser = await _anwsersCollection
				.Find(a => a.Id == objectId && a.UserEmail == userEmail)
				.FirstOrDefaultAsync();

			if (existingAnwser == null)
				return NotFound(new { error = "Answer not found or you do not have access." });

			anwser.CreationDate = existingAnwser.CreationDate;
			anwser.UpdateDate = DateTime.UtcNow;

			var result = await _anwsersCollection.ReplaceOneAsync(
				a => a.Id == objectId && a.UserEmail == userEmail,
				anwser
			);

			if (result.MatchedCount == 0)
				return NotFound(new { error = "Answer no longer exists." });

			return NoContent();
		}

		[HttpPost]
		public async Task<ActionResult<Answer>> PostAnwser(Answer anwser)
		{
			anwser.UpdateDate = null;
			anwser.CreationDate = DateTime.UtcNow;

			await _anwsersCollection.InsertOneAsync(anwser);

			return CreatedAtAction(nameof(GetAnwser), new { id = anwser.Id.ToString() }, anwser);
		}

		[HttpDelete("me/{id}")]
		public async Task<IActionResult> DeleteAnwser(string id)
		{
			var userEmail = GetCurrentUserEmail();

			if (!ObjectId.TryParse(id, out var objectId))
				return BadRequest(new { error = "Invalid Answer ID format." });

			var result = await _anwsersCollection.DeleteOneAsync(
				a => a.Id == objectId && a.UserEmail == userEmail
			);

			if (result.DeletedCount == 0)
				return NotFound(new { error = "Answer not found or you do not have access." });

			return NoContent();
		}

		private string GetCurrentUserEmail()
		{
			return User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value
				   ?? throw new UnauthorizedAccessException("User is not authenticated.");
		}
	}
}


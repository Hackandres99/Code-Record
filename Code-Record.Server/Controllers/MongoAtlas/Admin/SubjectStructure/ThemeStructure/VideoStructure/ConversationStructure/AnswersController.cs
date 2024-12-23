using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Bson;
using Code_Record.Server.Models.MongoAtlas.SubjectStructure.ThemeStructure.VideoStructure.ConversationStructure;

namespace Code_Record.Server.Controllers.MongoAtlas.Admin.SubjectStructure.ThemeStructure.VideoStructure.ConversationStructure
{
	[Route("api/mongo/admin/[controller]")]
	[ApiController]
	public class AnswersController : ControllerBase
	{
		private readonly IMongoCollection<Answer> _answersCollection;

		public AnswersController(IMongoDatabase database)
		{
			_answersCollection = database.GetCollection<Answer>("Answers");
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> PutAnswer(string id, Answer answer)
		{
			if (!ObjectId.TryParse(id, out var answerId))
				return BadRequest(new { error = "Invalid answer ID format." });

			if (id != answer.Id.ToString())
				return BadRequest(new { error = "Answer ID does not match." });

			var existingAnswer = await _answersCollection.Find(a => a.Id == answerId).FirstOrDefaultAsync();
			if (existingAnswer == null) return NotFound();

			answer.CreationDate = existingAnswer.CreationDate;
			answer.UpdateDate = DateTime.UtcNow;

			var result = await _answersCollection.ReplaceOneAsync(
				a => a.Id == answerId,
				answer
			);

			if (result.MatchedCount == 0)
				return NotFound();

			return NoContent();
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteAnswer(string id)
		{
			if (!ObjectId.TryParse(id, out var answerId))
				return BadRequest(new { error = "Invalid answer ID format." });

			var result = await _answersCollection.DeleteOneAsync(a => a.Id == answerId);

			if (result.DeletedCount == 0)
				return NotFound();

			return NoContent();
		}
	}
}


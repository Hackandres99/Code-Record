using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Code_Record.Server.Models.MongoAtlas.SubjectStructure.TestStructure;
using MongoDB.Bson;
using Code_Record.Server.Extensions.Controllers;

namespace Code_Record.Server.Controllers.MongoAtlas.Admin.SubjectStructure.TestStructure
{
	[Route("api/mongo/admin/[controller]")]
	[ApiController]
	public class QuestionsController : ControllerBase
	{
		private readonly IMongoCollection<Question> _questionsCollection;
		private readonly MongoDeleteService _deleteService;
		private readonly MongoPostService _postService;

		public QuestionsController(
			IMongoDatabase database,
			MongoDeleteService deleteService,
			MongoPostService postService)
		{
			_questionsCollection = database.GetCollection<Question>("Questions");
			_deleteService = deleteService;
			_postService = postService;
		}

		[HttpGet]
		public async Task<ActionResult<IEnumerable<Question>>> GetQuestions()
		{
			var questions = await _questionsCollection.Find(_ => true).ToListAsync();
			return Ok(questions);
		}

		[HttpGet("{id}")]
		public async Task<ActionResult<Question>> GetQuestion(string id)
		{
			if (!ObjectId.TryParse(id, out var objectId)) 
				return BadRequest(new { error = "Invalid Question ID format." });
			var question = await _questionsCollection.Find(q => q.Id == objectId).FirstOrDefaultAsync();
			if (question == null) return NotFound();
			return Ok(question);
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> PutQuestion(string id, Question question)
		{
			if (!ObjectId.TryParse(id, out var objectId)) 
				return BadRequest(new { error = "Invalid Question ID format." });
			var existingQuestion = await _questionsCollection.Find(q => q.Id == objectId).FirstOrDefaultAsync();
			if (existingQuestion == null) return NotFound();

			question.CreationDate = existingQuestion.CreationDate;
			question.UpdateDate = DateTime.UtcNow;

			var result = await _questionsCollection.ReplaceOneAsync(q => q.Id == objectId, question);
			if (result.MatchedCount == 0) return NotFound();
			return NoContent();
		}

		[HttpPost]
		public async Task<ActionResult<Question>> PostQuestion(Question question)
		{
			var result = await _postService.CreateQuestionAsync(question);
			if (string.IsNullOrEmpty(result)) return Ok(new { message = "Question created successfully." });
			return NotFound(new { error = result });
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteQuestion(string id)
		{
			var deleteResult = await _deleteService.QuestionAsync(id);
			if (string.IsNullOrEmpty(deleteResult)) return NoContent(); 
			return BadRequest(new { error = deleteResult });
		}
	}
}


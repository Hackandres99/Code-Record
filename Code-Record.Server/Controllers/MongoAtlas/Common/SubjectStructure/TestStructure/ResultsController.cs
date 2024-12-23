using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Bson;
using Code_Record.Server.Models.MongoAtlas.SubjectStructure.TestStructure;
using System.Security.Claims;

namespace Code_Record.Server.Controllers.MongoAtlas.Common.SubjectStructure.TestStructure
{
	[Route("api/mongo/common/[controller]")]
	[ApiController]
	public class ResultsController : ControllerBase
	{
		private readonly IMongoCollection<Result> _resultsCollection;

		public ResultsController(IMongoDatabase database)
		{
			_resultsCollection = database.GetCollection<Result>("Results");
		}

		[HttpGet("me")]
		public async Task<ActionResult<IEnumerable<Result>>> GetResults()
		{
			var userEmail = GetCurrentUserEmail();
			var results = await _resultsCollection
				.Find(r => r.UserEmail == userEmail)
				.ToListAsync();

			return Ok(results);
		}

		[HttpGet("me/{id}")]
		public async Task<ActionResult<Result>> GetResult(string id)
		{
			var userEmail = GetCurrentUserEmail();

			if (!ObjectId.TryParse(id, out var resultId))
				return BadRequest(new { error = "Invalid Result ID format." });

			var result = await _resultsCollection
				.Find(r => r.Id == resultId && r.UserEmail == userEmail)
				.FirstOrDefaultAsync();

			if (result == null)
				return NotFound(new { error = "Result not found or you do not have access." });

			return Ok(result);
		}

		[HttpPut("me/{id}")]
		public async Task<IActionResult> PutResult(string id, Result result)
		{
			var userEmail = GetCurrentUserEmail();

			if (!ObjectId.TryParse(id, out var resultId))
				return BadRequest(new { error = "Invalid Result ID format." });

			if (result.Id != resultId)
				return BadRequest(new { error = "Result ID does not match." });

			var existingResult = await _resultsCollection
				.Find(r => r.Id == resultId && r.UserEmail == userEmail)
				.FirstOrDefaultAsync();

			if (existingResult == null)
				return NotFound(new { error = "Result not found or you do not have access." });

			result.UploadDate = existingResult.UploadDate;

			var updateResult = await _resultsCollection.ReplaceOneAsync(
				r => r.Id == resultId && r.UserEmail == userEmail,
				result
			);

			if (updateResult.ModifiedCount == 0)
				return NotFound(new { error = "Result no longer exists." });

			return NoContent();
		}

		[HttpPost]
		public async Task<ActionResult<Result>> PostResult(Result result)
		{
			result.UploadDate = DateTime.UtcNow;
			await _resultsCollection.InsertOneAsync(result);

			return CreatedAtAction(nameof(GetResult), new { id = result.Id.ToString() }, result);
		}

		private string GetCurrentUserEmail()
		{
			var emailClaim = User.FindFirst(ClaimTypes.Email)?.Value;

			if (string.IsNullOrEmpty(emailClaim))
				throw new UnauthorizedAccessException("Email not found in claims.");

			return emailClaim;
		}
	}
}


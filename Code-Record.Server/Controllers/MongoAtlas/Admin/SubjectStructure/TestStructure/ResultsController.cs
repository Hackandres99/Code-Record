using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Code_Record.Server.Models.MongoAtlas.SubjectStructure.TestStructure;
using MongoDB.Bson;

namespace Code_Record.Server.Controllers.MongoAtlas.Admin.SubjectStructure.TestStructure
{
	[Route("api/mongo/admin/[controller]")]
	[ApiController]
	public class ResultsController : ControllerBase
	{
		private readonly IMongoCollection<Result> _resultsCollection;

		public ResultsController(IMongoDatabase database)
		{
			_resultsCollection = database.GetCollection<Result>("Results");
		}

		[HttpGet]
		public async Task<ActionResult<IEnumerable<Result>>> GetResults()
		{
			var results = await _resultsCollection.Find(_ => true).ToListAsync();
			return Ok(results);
		}

		[HttpGet("{id}")]
		public async Task<ActionResult<Result>> GetResult(string id)
		{
			if (!ObjectId.TryParse(id, out var objectId))
				return BadRequest(new { error = "Invalid Result ID format." });
			var result = await _resultsCollection.Find(r => r.Id == objectId).FirstOrDefaultAsync();
			if (result == null) return NotFound();
			return Ok(result);
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> PutResult(string id, Result result)
		{
			if (!ObjectId.TryParse(id, out var objectId))
				return BadRequest(new { error = "Invalid Result ID format." });

			var existingResult = await _resultsCollection.Find(r => r.Id == objectId).FirstOrDefaultAsync();
			if (existingResult == null) return NotFound();
			result.UploadDate = existingResult.UploadDate;

			var updateResult = await _resultsCollection.ReplaceOneAsync(r => r.Id == objectId, result);
			if (updateResult.MatchedCount == 0) return NotFound();
			return NoContent();
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteResult(string id)
		{
			if (!ObjectId.TryParse(id, out var objectId))
				return BadRequest(new { error = "Invalid Result ID format." });

			var result = await _resultsCollection.DeleteOneAsync(r => r.Id == objectId);
			if (result.DeletedCount == 0) return NotFound();
			return NoContent();
		}
	}
}


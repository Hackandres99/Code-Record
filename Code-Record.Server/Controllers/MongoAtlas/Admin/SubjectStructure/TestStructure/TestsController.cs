using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Code_Record.Server.Models.MongoAtlas.SubjectStructure.TestStructure;
using MongoDB.Bson;
using Code_Record.Server.Models.MongoAtlas.SubjectStructure;
using Code_Record.Server.Extensions.Controllers;

namespace Code_Record.Server.Controllers.MongoAtlas.Admin.SubjectStructure.TestStructure
{
	[Route("api/mongo/admin/[controller]")]
	[ApiController]
	public class TestsController : ControllerBase
	{
		private readonly IMongoCollection<Test> _testsCollection;
		private readonly MongoDeleteService _deleteService;
		private readonly MongoPostService _postService;

		public TestsController(
			IMongoDatabase database,
			MongoDeleteService deleteService,
			MongoPostService postService)
		{
			_testsCollection = database.GetCollection<Test>("Tests");
			_deleteService = deleteService;
			_postService = postService;
		}

		[HttpGet]
		public async Task<ActionResult<IEnumerable<Test>>> GetTests()
		{
			var tests = await _testsCollection.Find(_ => true).ToListAsync();
			return Ok(tests);
		}

		[HttpGet("{id}")]
		public async Task<ActionResult<Test>> GetTest(string id)
		{
			if (!ObjectId.TryParse(id, out var objectId)) 
				return BadRequest(new { error = "Invalid Test ID format." });
			var test = await _testsCollection.Find(t => t.Id == objectId).FirstOrDefaultAsync();
			if (test == null) return NotFound();
			return Ok(test);
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> PutTest(string id, Test test)
		{
			if (!ObjectId.TryParse(id, out var objectId))
				return BadRequest(new { error = "Invalid Test ID format." });

			var existingTest = await _testsCollection.Find(t => t.Id == objectId).FirstOrDefaultAsync();

			if (existingTest == null) return NotFound();
			test.CreationDate = existingTest.CreationDate;

			test.UpdateDate = DateTime.UtcNow;

			var result = await _testsCollection.ReplaceOneAsync(t => t.Id == objectId, test);
			if (result.MatchedCount == 0) return NotFound();
			return NoContent();
		}

		[HttpPost]
		public async Task<ActionResult<Test>> PostTest(Test test)
		{
			var result = await _postService.CreateTestAsync(test);
			if (string.IsNullOrEmpty(result)) return Ok(new { message = "Test created successfully." });
			return NotFound(new { error = result });
		}


		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteTest(string id)
		{
			var deleteResult = await _deleteService.TestAsync(id);
			if (string.IsNullOrEmpty(deleteResult)) return NoContent(); 
			return BadRequest(new { error = deleteResult });
		}
	}
}


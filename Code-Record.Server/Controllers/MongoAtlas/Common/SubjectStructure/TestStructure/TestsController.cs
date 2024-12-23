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
	public class TestsController : ControllerBase
	{
		private readonly IMongoCollection<Test> _testsCollection;
		private readonly IMongoCollection<Allow> _allowsCollection;

		public TestsController(IMongoDatabase database)
		{
			_testsCollection = database.GetCollection<Test>("Tests");
			_allowsCollection = database.GetCollection<Allow>("Allows");
		}

		[HttpGet("me")]
		public async Task<ActionResult<IEnumerable<Test>>> GetTests()
		{
			var userId = GetCurrentUserId();
			if (userId == null) return Unauthorized();

			// Obtener los IDs de los tests permitidos
			var allowedTestIds = await _allowsCollection
				.Find(a => a.UserId == userId && a.ResourceType == "test")
				.Project(a => a.ResourceId)
				.ToListAsync();

			if (!allowedTestIds.Any()) return Ok(new List<Test>());

			// Consultar los tests permitidos
			var tests = await _testsCollection
				.Find(t => allowedTestIds.Contains(t.Id))
				.ToListAsync();

			return Ok(tests);
		}

		[HttpGet("me/{id}")]
		public async Task<ActionResult<Test>> GetTest(string id)
		{
			var userId = GetCurrentUserId();
			if (userId == null) return Unauthorized();

			if (!ObjectId.TryParse(id, out var objectId))
				return BadRequest("Invalid test ID format.");

			// Verificar si el usuario tiene permiso para este test
			var isAllowed = await _allowsCollection
				.Find(a => a.UserId == userId && a.ResourceType == "test" && a.ResourceId == objectId)
				.AnyAsync();

			if (!isAllowed) return Forbid();

			// Consultar el test
			var test = await _testsCollection
				.Find(t => t.Id == objectId)
				.FirstOrDefaultAsync();

			if (test == null) return NotFound();

			return Ok(test);
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


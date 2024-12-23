using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Bson;
using Code_Record.Server.Models.MongoAtlas;

namespace Code_Record.Server.Controllers.MongoAtlas.Admin
{
	[Route("api/mongo/admin/[controller]")]
	[ApiController]
	public class VisitsController : ControllerBase
	{
		private readonly IMongoCollection<Visit> _visitsCollection;

		public VisitsController(IMongoDatabase database)
		{
			_visitsCollection = database.GetCollection<Visit>("Visits");
		}

		[HttpGet]
		public async Task<ActionResult<IEnumerable<Visit>>> GetVisits()
		{
			var visits = await _visitsCollection.Find(_ => true).ToListAsync();
			return Ok(visits);
		}

		[HttpGet("{id}")]
		public async Task<ActionResult<Visit>> GetVisit(string id)
		{
			if (!ObjectId.TryParse(id, out var visitId))
				return BadRequest(new { error = "Invalid Visit ID format." });

			var visit = await _visitsCollection.Find(v => v.Id == visitId).FirstOrDefaultAsync();
			if (visit == null) return NotFound();

			return Ok(visit);
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> PutVisit(string id, Visit visit)
		{
			if (!ObjectId.TryParse(id, out var visitId))
				return BadRequest(new { error = "Invalid Visit ID format." });

			if (id != visit.Id.ToString())
				return BadRequest(new { error = "Visit ID does not match." });

			var existingVisit = await _visitsCollection
				.Find(v => v.Id == visitId)
				.FirstOrDefaultAsync();

			if (existingVisit == null) return NotFound(new { error = "Visit not found." });
			visit.VisitDate = existingVisit.VisitDate;

			var result = await _visitsCollection.ReplaceOneAsync(v => v.Id == visitId, visit);
			if (result.MatchedCount == 0) return NotFound(new { error = "Visit no longer exists." });
			return NoContent();
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteVisit(string id)
		{
			if (!ObjectId.TryParse(id, out var visitId))
				return BadRequest(new { error = "Invalid Visit ID format." });

			var result = await _visitsCollection.DeleteOneAsync(v => v.Id == visitId);
			if (result.DeletedCount == 0) return NotFound(new { error = "Visit not found." });
			return NoContent();
		}
	}
}


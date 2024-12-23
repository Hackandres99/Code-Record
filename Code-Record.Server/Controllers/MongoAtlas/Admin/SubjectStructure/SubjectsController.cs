using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Code_Record.Server.Models.MongoAtlas.SubjectStructure;
using MongoDB.Bson;
using Code_Record.Server.Extensions.Controllers;

namespace Code_Record.Server.Controllers.MongoAtlas.Admin.SubjectStructure
{
	[Route("api/mongo/admin/[controller]")]
	[ApiController]
	public class SubjectsController : ControllerBase
	{
		private readonly IMongoCollection<Subject> _subjectsCollection;
		private readonly MongoDeleteService _deleteService;

		public SubjectsController(IMongoDatabase database, MongoDeleteService deleteService)
		{
			_subjectsCollection = database.GetCollection<Subject>("Subjects");
			_deleteService = deleteService;
		}

		[HttpGet]
		public async Task<ActionResult<IEnumerable<Subject>>> GetSubjects()
		{
			var subjects = await _subjectsCollection.Find(_ => true).ToListAsync();
			return Ok(subjects);
		}

		[HttpGet("{id}")]
		public async Task<ActionResult<Subject>> GetSubject(string id)
		{
			if (!ObjectId.TryParse(id, out var subjectId))
				return BadRequest(new { error = "Invalid Subject ID format." });

			var subject = await _subjectsCollection.Find(s => s.Id == subjectId).FirstOrDefaultAsync();
			if (subject == null) return NotFound();

			return Ok(subject);
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> PutSubject(string id, Subject subject)
		{
			if (!ObjectId.TryParse(id, out var subjectId))
				return BadRequest(new { error = "Invalid Subject ID format." });

			if (id != subject.Id.ToString()) return BadRequest(new { error = "Subject ID does not match." });

			var existingSubject = await _subjectsCollection.Find(s => s.Id == subjectId).FirstOrDefaultAsync();
			if (existingSubject == null) return NotFound();

			subject.CreationDate = existingSubject.CreationDate;
			subject.UpdateDate = DateTime.UtcNow;

			var result = await _subjectsCollection.ReplaceOneAsync(s => s.Id == subjectId, subject);
			if (result.MatchedCount == 0) return NotFound();
			return NoContent();
		}

		[HttpPost]
		public async Task<ActionResult<Subject>> PostSubject(Subject subject)
		{
			subject.CreationDate = DateTime.UtcNow;
			subject.UpdateDate = null;
			await _subjectsCollection.InsertOneAsync(subject);
			return CreatedAtAction("GetSubject", new { id = subject.Id }, subject);
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteSubject(string id)
		{
			var deleteResult = await _deleteService.SubjectsAsync(id);
			if (!string.IsNullOrEmpty(deleteResult)) return BadRequest(new { error = deleteResult });
			return NoContent();
		}
	}
}

using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Code_Record.Server.Models.MongoAtlas;
using System.Security.Claims;

namespace Code_Record.Server.Controllers.MongoAtlas.Common
{
	[Route("api/mongo/common/[controller]")]
	[ApiController]
	public class VisitsController : ControllerBase
	{
		private readonly IMongoCollection<Visit> _visitsCollection;

		public VisitsController(IMongoDatabase database)
		{
			_visitsCollection = database.GetCollection<Visit>("Visits");
		}

		[HttpGet("count")]
		public async Task<ActionResult<int>> GetVisits()
		{
			var count = await _visitsCollection.CountDocumentsAsync(FilterDefinition<Visit>.Empty);
			return Ok((int)count);
		}

		[HttpPost]
		public async Task<ActionResult<Visit>> PostVisit()
		{
			var visit = new Visit
			{
				VisitDate = DateTime.UtcNow,
				UserEmail = GetCurrentUserEmail()
			};

			await _visitsCollection.InsertOneAsync(visit);

			return CreatedAtAction("GetVisit", new { id = visit.Id }, visit);
		}

		private string GetCurrentUserEmail()
		{
			return User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value
				   ?? throw new UnauthorizedAccessException("User is not authenticated.");
		}
	}
}

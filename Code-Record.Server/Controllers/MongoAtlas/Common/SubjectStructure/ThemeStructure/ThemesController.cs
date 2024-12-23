using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Bson;
using Code_Record.Server.Models.MongoAtlas;
using Code_Record.Server.Models.MongoAtlas.SubjectStructure.ThemeStructure;
using System.Security.Claims;

namespace Code_Record.Server.Controllers.MongoAtlas.Common.SubjectStructure.ThemeStructure
{
	[Route("api/mongo/common/[controller]")]
	[ApiController]
	public class ThemesController : ControllerBase
	{
		private readonly IMongoCollection<Theme> _themesCollection;
		private readonly IMongoCollection<Allow> _allowsCollection;

		public ThemesController(IMongoDatabase database)
		{
			_themesCollection = database.GetCollection<Theme>("Themes");
			_allowsCollection = database.GetCollection<Allow>("Allows");
		}

		[HttpGet("me")]
		public async Task<ActionResult<IEnumerable<Theme>>> GetThemes()
		{
			var userId = GetCurrentUserId();
			if (userId == null) return Unauthorized();

			var allowedThemeIds = await _allowsCollection
				.Find(a => a.UserId == userId && a.ResourceType == "theme")
				.Project(a => a.ResourceId)
				.ToListAsync();

			if (!allowedThemeIds.Any()) return Ok(new List<Theme>());

			var themes = await _themesCollection
				.Find(t => allowedThemeIds.Contains(t.Id))
				.ToListAsync();

			return Ok(themes);
		}

		[HttpGet("me/{id}")]
		public async Task<ActionResult<Theme>> GetTheme(string id)
		{
			var userId = GetCurrentUserId();
			if (userId == null) return Unauthorized();

			if (!ObjectId.TryParse(id, out var objectId))
				return BadRequest("Invalid theme ID format.");

			var isAllowed = await _allowsCollection
				.Find(a => a.UserId == userId && a.ResourceType == "theme" && a.ResourceId == objectId)
				.AnyAsync();

			if (!isAllowed) return Forbid();

			var theme = await _themesCollection
				.Find(t => t.Id == objectId)
				.FirstOrDefaultAsync();

			if (theme == null) return NotFound();

			return Ok(theme);
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


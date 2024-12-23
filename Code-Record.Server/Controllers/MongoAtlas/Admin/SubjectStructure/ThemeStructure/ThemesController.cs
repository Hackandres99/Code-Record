using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Code_Record.Server.Models.MongoAtlas.SubjectStructure.ThemeStructure;
using MongoDB.Bson;
using Code_Record.Server.Extensions.Controllers;

namespace Code_Record.Server.Controllers.MongoAtlas.Admin.SubjectStructure.ThemeStructure
{
	[Route("api/mongo/admin/[controller]")]
	[ApiController]
	public class ThemesController : ControllerBase
	{
		private readonly IMongoCollection<Theme> _themesCollection;
		private readonly MongoDeleteService _deleteService;
		private readonly MongoPostService _postService;

		public ThemesController(
			IMongoDatabase database,
			MongoDeleteService deleteService,
			MongoPostService postService)
		{
			_themesCollection = database.GetCollection<Theme>("Themes");
			_deleteService = deleteService;
			_postService = postService;
		}

		[HttpGet]
		public async Task<ActionResult<IEnumerable<Theme>>> GetThemes()
		{
			var themes = await _themesCollection.Find(_ => true).ToListAsync();
			return Ok(themes);
		}

		[HttpGet("{id}")]
		public async Task<ActionResult<Theme>> GetTheme(string id)
		{
			if (!ObjectId.TryParse(id, out var themeId))
				return BadRequest(new { error = "Invalid Theme ID format." });

			var theme = await _themesCollection.Find(t => t.Id == themeId).FirstOrDefaultAsync();
			if (theme == null) return NotFound();

			return Ok(theme);
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> PutTheme(string id, Theme theme)
		{
			if (!ObjectId.TryParse(id, out var themeId))
				return BadRequest(new { error = "Invalid Theme ID format." });

			if (id != theme.Id.ToString())
				return BadRequest(new { error = "Theme ID does not match." });

			var existingTheme = await _themesCollection.Find(t => t.Id == themeId).FirstOrDefaultAsync();
			if (existingTheme == null) return NotFound();
			theme.CreationDate = existingTheme.CreationDate;
			theme.UpdateDate = DateTime.UtcNow;

			var result = await _themesCollection.ReplaceOneAsync(t => t.Id == themeId, theme);
			if (result.MatchedCount == 0) return NotFound();
			return NoContent();
		}

		[HttpPost]
		public async Task<ActionResult<Theme>> PostTheme(Theme theme)
		{
			var result = await _postService.CreateThemeAsync(theme);
			if (string.IsNullOrEmpty(result)) return Ok(new { message = "Theme created successfully." });
			return NotFound(new { error = result });
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteTheme(string id)
		{
			var deleteResult = await _deleteService.ThemeAsync(id);
			if (string.IsNullOrEmpty(deleteResult)) return NoContent(); 
			return BadRequest(new { error = deleteResult });		
		}
	}
}


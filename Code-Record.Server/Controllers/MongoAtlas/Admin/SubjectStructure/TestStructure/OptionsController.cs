using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Code_Record.Server.Models.MongoAtlas.SubjectStructure.TestStructure;
using MongoDB.Bson;
using Code_Record.Server.Extensions.Controllers;

namespace Code_Record.Server.Controllers.MongoAtlas.Admin.SubjectStructure.TestStructure
{
	[Route("api/mongo/admin/[controller]")]
	[ApiController]
	public class OptionsController : ControllerBase
	{
		private readonly IMongoCollection<Option> _optionsCollection;
		private readonly MongoDeleteService _deleteService;
		private readonly MongoPostService _postService;

		public OptionsController(
			IMongoDatabase database, 
			MongoDeleteService deleteService,
			MongoPostService postService)
		{
			_optionsCollection = database.GetCollection<Option>("Options");
			_deleteService = deleteService;
			_postService = postService;
		}

		[HttpGet]
		public async Task<ActionResult<IEnumerable<Option>>> GetOptions()
		{
			var options = await _optionsCollection.Find(_ => true).ToListAsync();
			return Ok(options);
		}

		[HttpGet("{id}")]
		public async Task<ActionResult<Option>> GetOption(string id)
		{
			if (!ObjectId.TryParse(id, out var objectId)) 
				return BadRequest(new { error = "Invalid Option ID format." });
			var option = await _optionsCollection.Find(o => o.Id == objectId).FirstOrDefaultAsync();
			if (option == null) return NotFound();
			return Ok(option);
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> PutOption(string id, Option option)
		{
			if (!ObjectId.TryParse(id, out var objectId)) 
				return BadRequest(new { error = "Invalid Option ID format." });

			var existingOption = await _optionsCollection.Find(o => o.Id == objectId).FirstOrDefaultAsync();
			if (existingOption == null) return NotFound();

			option.CreationDate = existingOption.CreationDate;
			option.UpdateDate = DateTime.UtcNow;

			var result = await _optionsCollection.ReplaceOneAsync(o => o.Id == objectId, option);

			if (result.MatchedCount == 0) return NotFound();
			return NoContent();
		}

		[HttpPost]
		public async Task<ActionResult<Option>> PostOption(Option option)
		{
			var result = await _postService.CreateOptionAsync(option);
			if (string.IsNullOrEmpty(result)) return Ok(new { message = "Option created successfully." });
			return NotFound(new { error = result });
		}


		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteOption(string id)
		{
			var deleteResult = await _deleteService.VideoAsync(id);
			if (string.IsNullOrEmpty(deleteResult)) return NoContent(); 
			return BadRequest(new { error = deleteResult });
		}
	}
}


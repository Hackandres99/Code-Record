using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Code_Record.Server.Models.MongoAtlas.SubjectStructure;
using MongoDB.Bson;
using Code_Record.Server.Extensions.Controllers;

namespace Code_Record.Server.Controllers.MongoAtlas.Admin.SubjectStructure
{
	[Route("api/mongo/admin/[controller]")]
	[ApiController]
	public class ResourcesController : ControllerBase
	{
		private readonly IMongoCollection<Resource> _resourcesCollection;
		private readonly MongoDeleteService _deleteService;
		private readonly MongoPostService _postService;

		public ResourcesController(
			IMongoDatabase database,
			MongoDeleteService deleteService,
			MongoPostService postService)
		{
			_resourcesCollection = database.GetCollection<Resource>("Resources");
			_deleteService = deleteService;
			_postService = postService;
		}

		[HttpGet]
		public async Task<ActionResult<IEnumerable<Resource>>> GetResources()
		{
			var resources = await _resourcesCollection.Find(_ => true).ToListAsync();
			return Ok(resources);
		}

		[HttpGet("{id}")]
		public async Task<ActionResult<Resource>> GetResource(string id)
		{
			if (!ObjectId.TryParse(id, out var objectId))
				return BadRequest(new { error = "Invalid Resource ID format." });

			var resource = await _resourcesCollection.Find(r => r.Id == objectId).FirstOrDefaultAsync();
			if (resource == null) return NotFound();

			return Ok(resource);
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> PutResource(string id, Resource resource)
		{
			if (!ObjectId.TryParse(id, out var objectId))
				return BadRequest(new { error = "Invalid Resource ID format." });

			if (id != resource.Id.ToString())
				return BadRequest(new { error = "Resource ID does not match." });

			var existingResource = await _resourcesCollection.Find(r => r.Id == objectId).FirstOrDefaultAsync();
			if (existingResource == null) return NotFound();

			resource.UploadDate = existingResource.UploadDate;
			resource.UpdateDate = DateTime.UtcNow;

			var result = await _resourcesCollection.ReplaceOneAsync(r => r.Id == objectId, resource);
			if (result.MatchedCount == 0) return NotFound();

			return NoContent();
		}

		[HttpPost]
		public async Task<ActionResult<Resource>> PostResource(Resource resource)
		{
			var result = await _postService.CreateResourceAsync(resource);
			if (string.IsNullOrEmpty(result)) return Ok(new { message = "Resource created successfully." });
			return NotFound(new { error = result });
		}


		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteResource(string id)
		{
			var deleteResult = await _deleteService.ResourceAsync(id);
			if (string.IsNullOrEmpty(deleteResult)) return NoContent(); 
			return BadRequest(new { error = deleteResult });
		}
	}
}


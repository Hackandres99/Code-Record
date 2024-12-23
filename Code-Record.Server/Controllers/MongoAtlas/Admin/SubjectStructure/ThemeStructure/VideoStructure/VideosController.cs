using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Code_Record.Server.Models.MongoAtlas.SubjectStructure.ThemeStructure.VideoStructure;
using MongoDB.Bson;
using Code_Record.Server.Extensions.Controllers;

namespace Code_Record.Server.Controllers.MongoAtlas.Admin.SubjectStructure.ThemeStructure.VideoStructure
{
	[Route("api/mongo/admin/[controller]")]
	[ApiController]
	public class VideosController : ControllerBase
	{
		private readonly IMongoCollection<Video> _videosCollection;
		private readonly MongoDeleteService _deleteService;
		private readonly MongoPostService _postService;

		public VideosController(
			IMongoDatabase database,
			MongoDeleteService deleteService,
			MongoPostService postService)
		{
			_videosCollection = database.GetCollection<Video>("Videos");
			_deleteService = deleteService;
			_postService = postService;
		}

		[HttpGet]
		public async Task<ActionResult<IEnumerable<Video>>> GetVideos()
		{
			var videos = await _videosCollection.Find(_ => true).ToListAsync();
			return Ok(videos);
		}

		[HttpGet("{id}")]
		public async Task<ActionResult<Video>> GetVideo(string id)
		{
			if (!ObjectId.TryParse(id, out var videoId)) return BadRequest(new { error = "Invalid Video ID format." });
			var video = await _videosCollection.Find(v => v.Id == videoId).FirstOrDefaultAsync();
			if (video == null) return NotFound();
			return Ok(video);
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> PutVideo(string id, Video video)
		{
			if (!ObjectId.TryParse(id, out var videoId)) return BadRequest(new { error = "Invalid Video ID format." });
			if (id != video.Id.ToString()) return BadRequest(new { error = "Video ID does not match." });

			var existingVideo = await _videosCollection.Find(v => v.Id == videoId).FirstOrDefaultAsync();
			if (existingVideo == null) return NotFound();

			video.UploadDate = existingVideo.UploadDate;
			video.UpdateDate = DateTime.UtcNow;
			var result = await _videosCollection.ReplaceOneAsync(v => v.Id == videoId, video);

			if (result.MatchedCount == 0) return NotFound();
			return NoContent();
		}

		[HttpPost]
		public async Task<ActionResult<Video>> PostVideo(Video video)
		{
			var result = await _postService.CreateVideoAsync(video);
			if (string.IsNullOrEmpty(result)) return Ok(new { message = "Video created successfully." });
			return NotFound(new { error = result });
		}


		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteVideo(string id)
		{
			var deleteResult = await _deleteService.VideoAsync(id);
			if (string.IsNullOrEmpty(deleteResult)) return NoContent(); 
			return BadRequest(new { error = deleteResult });
		}
	}
}


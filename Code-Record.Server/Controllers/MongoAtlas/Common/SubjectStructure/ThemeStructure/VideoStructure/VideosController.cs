using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Bson;
using Code_Record.Server.Models.MongoAtlas;
using Code_Record.Server.Models.MongoAtlas.SubjectStructure.ThemeStructure.VideoStructure;
using System.Security.Claims;

namespace Code_Record.Server.Controllers.MongoAtlas.Common.SubjectStructure.ThemeStructure.VideoStructure
{
	[Route("api/mongo/common/[controller]")]
	[ApiController]
	public class VideosController : ControllerBase
	{
		private readonly IMongoCollection<Video> _videosCollection;
		private readonly IMongoCollection<Allow> _allowsCollection;

		public VideosController(IMongoDatabase database)
		{
			_videosCollection = database.GetCollection<Video>("Videos");
			_allowsCollection = database.GetCollection<Allow>("Allows");
		}

		[HttpGet("me")]
		public async Task<ActionResult<IEnumerable<Video>>> GetVideos()
		{
			var userId = GetCurrentUserId();
			if (userId == null) return Unauthorized();

			var allowedVideoIds = await _allowsCollection
				.Find(a => a.UserId == userId && a.ResourceType == "video")
				.Project(a => a.ResourceId)
				.ToListAsync();

			if (!allowedVideoIds.Any()) return Ok(new List<Video>());

			var videos = await _videosCollection
				.Find(v => allowedVideoIds.Contains(v.Id))
				.ToListAsync();

			return Ok(videos);
		}

		[HttpGet("me/{id}")]
		public async Task<ActionResult<Video>> GetVideo(string id)
		{
			var userId = GetCurrentUserId();
			if (userId == null) return Unauthorized();

			if (!ObjectId.TryParse(id, out var objectId))
				return BadRequest("Invalid video ID format.");

			var isAllowed = await _allowsCollection
				.Find(a => a.UserId == userId && a.ResourceType == "video" && a.ResourceId == objectId)
				.AnyAsync();

			if (!isAllowed) return Forbid();

			var video = await _videosCollection
				.Find(v => v.Id == objectId)
				.FirstOrDefaultAsync();

			if (video == null) return NotFound();

			return Ok(video);
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


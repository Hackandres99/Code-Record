using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Bson;
using Code_Record.Server.Models.MongoAtlas.SubjectStructure.ThemeStructure.VideoStructure.ConversationStructure;
using System.Security.Claims;

namespace Code_Record.Server.Controllers.MongoAtlas.Common.SubjectStructure.ThemeStructure.VideoStructure.ConversationStructure
{
	[Route("api/mongo/common/[controller]")]
	[ApiController]
	public class CommentsController : ControllerBase
	{
		private readonly IMongoCollection<Comment> _commentsCollection;

		public CommentsController(IMongoDatabase database)
		{
			_commentsCollection = database.GetCollection<Comment>("Comments");
		}

		[HttpGet]
		public async Task<ActionResult<IEnumerable<Comment>>> GetComments()
		{
			var comments = await _commentsCollection.Find(_ => true).ToListAsync();
			return Ok(comments);
		}

		[HttpGet("{id}")]
		public async Task<ActionResult<Comment>> GetComment(string id)
		{
			if (!ObjectId.TryParse(id, out var objectId))
				return BadRequest(new { error = "Invalid Comment ID format." });

			var comment = await _commentsCollection.Find(c => c.Id == objectId).FirstOrDefaultAsync();

			if (comment == null) return NotFound();

			return Ok(comment);
		}

		[HttpPut("me/{id}")]
		public async Task<IActionResult> PutComment(string id, Comment comment)
		{
			var userEmail = GetCurrentUserEmail();

			if (!ObjectId.TryParse(id, out var objectId))
				return BadRequest(new { error = "Invalid Comment ID format." });

			if (comment.Id != objectId)
				return BadRequest(new { error = "Comment ID does not match." });

			var existingComment = await _commentsCollection
				.Find(c => c.Id == objectId && c.UserEmail == userEmail)
				.FirstOrDefaultAsync();

			if (existingComment == null)
				return NotFound(new { error = "Comment not found or you do not have access." });

			comment.CreationDate = existingComment.CreationDate;
			comment.UpdateDate = DateTime.UtcNow;

			var result = await _commentsCollection.ReplaceOneAsync(
				c => c.Id == objectId && c.UserEmail == userEmail,
				comment
			);

			if (result.MatchedCount == 0)
				return NotFound(new { error = "Comment no longer exists." });

			return NoContent();
		}

		[HttpPost]
		public async Task<ActionResult<Comment>> PostComment(Comment comment)
		{
			comment.UpdateDate = null;
			comment.CreationDate = DateTime.UtcNow;

			await _commentsCollection.InsertOneAsync(comment);

			return CreatedAtAction(nameof(GetComment), new { id = comment.Id.ToString() }, comment);
		}

		[HttpDelete("me/{id}")]
		public async Task<IActionResult> DeleteComment(string id)
		{
			var userEmail = GetCurrentUserEmail();

			if (!ObjectId.TryParse(id, out var objectId))
				return BadRequest(new { error = "Invalid Comment ID format." });

			var result = await _commentsCollection.DeleteOneAsync(
				c => c.Id == objectId && c.UserEmail == userEmail
			);

			if (result.DeletedCount == 0)
				return NotFound(new { error = "Comment not found or you do not have access." });

			return NoContent();
		}

		private string GetCurrentUserEmail()
		{
			return User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value
				   ?? throw new UnauthorizedAccessException("User is not authenticated.");
		}
	}
}


using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Code_Record.Server.Contexts;
using Code_Record.Server.Models.SQLServer.SubjectStructure.ThemeStructure.VideoStructure.ConversationStructure;
using System.Security.Claims;

namespace Code_Record.Server.Controllers.SQLServer.Common.SubjectStructure.ThemeStructure.VideoStructure.ConversationStructure
{
    [Route("api/sql/common/[controller]")]
    [ApiController]
    public class CommentsController : ControllerBase
    {
        private readonly SQLServerContext _context;

        public CommentsController(SQLServerContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Comment>>> GetComments()
        {
            return await _context.Comments.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Comment>> GetComment(Guid id)
        {
            var comment = await _context.Comments.FindAsync(id);

            if (comment == null) return NotFound();

			return comment;
        }

        [HttpPut("me/{id}")]
        public async Task<IActionResult> PutComment(Guid id, Comment comment)
        {
			var userEmail = GetCurrentUserEmail();

			if (id != comment.Id) 
				return BadRequest(new { error = "Comment ID does not match." });

			var existingComment = GetExistingComment(id, userEmail).Result;

			if (existingComment == null) 
				return NotFound(new { error = "Comment not found or you do not have access." });

			comment.CreationDate = existingComment.CreationDate;
			comment.UpdateDate = DateTime.UtcNow;
			_context.Entry(comment).State = EntityState.Modified;

			try
			{
				await _context.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!CommentExists(id)) 
                    return NotFound(new { error = "Comment no longer exists." });
				else throw;
			}

			return NoContent();
		}

        [HttpPost]
        public async Task<ActionResult<Comment>> PostComment(Comment comment)
        {
			comment.UpdateDate = null;
			comment.CreationDate = DateTime.UtcNow;
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetComment", new { id = comment.Id }, comment);
        }

        [HttpDelete("me/{id}")]
        public async Task<IActionResult> DeleteComment(Guid id)
        {
			var userEmail = GetCurrentUserEmail();

			var comment = await _context.Comments
				.FirstOrDefaultAsync(c => c.Id == id && c.UserEmail == userEmail);

			if (comment == null) 
                return NotFound(new { error = "Comment not found or you do not have access." });

			_context.Comments.Remove(comment);
			await _context.SaveChangesAsync();

			return NoContent();
		}

        private bool CommentExists(Guid id)
        {
            return _context.Comments.Any(e => e.Id == id);
        }

		private string GetCurrentUserEmail()
		{
			return User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value
				   ?? throw new UnauthorizedAccessException("User is not authenticated.");
		}

		private async Task<Comment> GetExistingComment(Guid id, string userEmail)
		{
			var existingComment = await _context.Comments
			.AsNoTracking()
			.FirstOrDefaultAsync(c => c.Id == id && c.UserEmail == userEmail);

			return existingComment;
		}
	}
}

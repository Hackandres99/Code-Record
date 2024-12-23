using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Code_Record.Server.Contexts;
using Code_Record.Server.Models.SQLServer.SubjectStructure.ThemeStructure.VideoStructure.ConversationStructure;

namespace Code_Record.Server.Controllers.SQLServer.Admin.SubjectStructure.ThemeStructure.VideoStructure.ConversationStructure
{
    [Route("api/sql/admin/[controller]")]
    [ApiController]
    public class CommentsController : ControllerBase
    {
        private readonly SQLServerContext _context;

        public CommentsController(SQLServerContext context)
        {
            _context = context;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutComment(Guid id, Comment comment)
        {
            if (id != comment.Id) return BadRequest();
            var existingComment = GetExistingComment(id).Result;
            comment.CreationDate = existingComment.CreationDate;
            comment.UpdateDate = DateTime.UtcNow;
			_context.Entry(comment).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CommentExists(id)) return NotFound();
				else throw;
			}

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComment(Guid id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null) return NotFound();

			_context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CommentExists(Guid id)
        {
            return _context.Comments.Any(e => e.Id == id);
        }

		private async Task<Comment> GetExistingComment(Guid id)
		{
			var existingComment = await _context.Comments
			.AsNoTracking()
			.FirstOrDefaultAsync(c => c.Id == id);

			return existingComment;
		}
	}
}

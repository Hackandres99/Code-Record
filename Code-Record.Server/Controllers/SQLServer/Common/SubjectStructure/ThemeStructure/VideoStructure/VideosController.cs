using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Code_Record.Server.Contexts;
using Code_Record.Server.Models.SQLServer.SubjectStructure.ThemeStructure.VideoStructure;
using System.Security.Claims;

namespace Code_Record.Server.Controllers.SQLServer.Common.SubjectStructure.ThemeStructure.VideoStructure
{
    [Route("api/sql/common/[controller]")]
    [ApiController]
    public class VideosController : ControllerBase
    {
        private readonly SQLServerContext _context;

        public VideosController(SQLServerContext context)
        {
            _context = context;
        }

        [HttpGet("me")]
        public async Task<ActionResult<IEnumerable<Video>>> GetVideos()
        {
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (userId == null) return Unauthorized();

			var videos = await _context.Videos
			.Where(v => _context.Allows
				.Any(a => a.UserId == Guid.Parse(userId) &&
						  a.ResourceId == v.Id &&
						  a.ResourceType == "video"))
			.ToListAsync();

			return Ok(videos);
		}

        [HttpGet("me/{id}")]
        public async Task<ActionResult<Video>> GetVideo(Guid id)
        {
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

			if (userId == null) return Unauthorized();

			var video = await _context.Videos
			.Where(v => v.Id == id && _context.Allows
				.Any(a => a.UserId == Guid.Parse(userId) &&
						  a.ResourceId == id &&
						  a.ResourceType == "video"))
			.FirstOrDefaultAsync();

			if (video == null) return Forbid();

			return Ok(video);
		}
    }
}

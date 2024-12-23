using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Code_Record.Server.Contexts;
using Code_Record.Server.Models.SQLServer.SubjectStructure.ThemeStructure.VideoStructure;

namespace Code_Record.Server.Controllers.SQLServer.Admin.SubjectStructure.ThemeStructure.VideoStructure
{
    [Route("api/sql/admin/[controller]")]
    [ApiController]
    public class VideosController : ControllerBase
    {
        private readonly SQLServerContext _context;

        public VideosController(SQLServerContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Video>>> GetVideos()
        {
            return await _context.Videos.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Video>> GetVideo(Guid id)
        {
            var video = await _context.Videos.FindAsync(id);

            if (video == null) return NotFound();

			return video;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutVideo(Guid id, Video video)
        {
            if (id != video.Id) return BadRequest();
            var existingVideo = GetExistingVideo(id).Result;
            video.UploadDate = existingVideo.UploadDate;
            video.UpdateDate = DateTime.UtcNow;
			_context.Entry(video).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VideoExists(id)) return NotFound();
				else throw;
			}

            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<Video>> PostVideo(Video video)
        {
            video.UploadDate = DateTime.UtcNow;
            video.UpdateDate = null;
            _context.Videos.Add(video);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetVideo", new { id = video.Id }, video);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVideo(Guid id)
        {
            var video = await _context.Videos.FindAsync(id);
            if (video == null) return NotFound();

			_context.Videos.Remove(video);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool VideoExists(Guid id)
        {
            return _context.Videos.Any(e => e.Id == id);
        }

		private async Task<Video> GetExistingVideo(Guid id)
		{
			var existingVideo = await _context.Videos
			.AsNoTracking()
			.FirstOrDefaultAsync(v => v.Id == id);

			return existingVideo;
		}
	}
}

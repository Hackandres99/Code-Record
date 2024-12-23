using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Code_Record.Server.Contexts;
using Code_Record.Server.Models.SQLServer.SubjectStructure.ThemeStructure;

namespace Code_Record.Server.Controllers.SQLServer.Admin.SubjectStructure.ThemeStructure
{
    [Route("api/sql/admin/[controller]")]
    [ApiController]
    public class ThemesController : ControllerBase
    {
        private readonly SQLServerContext _context;

        public ThemesController(SQLServerContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Theme>>> GetThemes()
        {
            return await _context.Themes.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Theme>> GetTheme(Guid id)
        {
            var theme = await _context.Themes.FindAsync(id);

            if (theme == null) return NotFound();

			return theme;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutTheme(Guid id, Theme theme)
        {
            if (id != theme.Id) return BadRequest();
            var existingTheme = GetExistingTheme(id).Result;
            theme.CreationDate = existingTheme.CreationDate;
            theme.UpdateDate = DateTime.UtcNow;
			_context.Entry(theme).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ThemeExists(id)) return NotFound();
				else throw;
			}

            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<Theme>> PostTheme(Theme theme)
        {
            theme.CreationDate = DateTime.UtcNow;
            theme.UpdateDate = null;
            _context.Themes.Add(theme);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTheme", new { id = theme.Id }, theme);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTheme(Guid id)
        {
            var theme = await _context.Themes.FindAsync(id);
            if (theme == null) return NotFound();

			_context.Themes.Remove(theme);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ThemeExists(Guid id)
        {
            return _context.Themes.Any(e => e.Id == id);
        }

		private async Task<Theme> GetExistingTheme(Guid id)
		{
			var existingTheme = await _context.Themes
			.AsNoTracking()
			.FirstOrDefaultAsync(t => t.Id == id);

			return existingTheme;
		}
	}
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Code_Record.Server.Contexts;
using Code_Record.Server.Models.SQLServer.SubjectStructure.TestStructure;

namespace Code_Record.Server.Controllers.SQLServer.Admin.SubjectStructure.TestStructure
{
    [Route("api/sql/admin/[controller]")]
    [ApiController]
    public class OptionsController : ControllerBase
    {
        private readonly SQLServerContext _context;

        public OptionsController(SQLServerContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Option>>> GetOptions()
        {
            return await _context.Options.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Option>> GetOption(Guid id)
        {
            var option = await _context.Options.FindAsync(id);

            if (option == null) return NotFound();

			return option;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutOption(Guid id, Option option)
        {
            if (id != option.Id) return BadRequest();
            var existingOption = GetExistingOption(id).Result;
            option.CreationDate = existingOption.CreationDate;
            option.UpdateDate = DateTime.UtcNow;
			_context.Entry(option).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OptionExists(id)) return NotFound();
				else throw;
			}

            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<Option>> PostOption(Option option)
        {
            option.CreationDate = DateTime.UtcNow;
            option.UpdateDate = null;
            _context.Options.Add(option);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetOption", new { id = option.Id }, option);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOption(Guid id)
        {
            var option = await _context.Options.FindAsync(id);
            if (option == null) return NotFound();

			_context.Options.Remove(option);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool OptionExists(Guid id)
        {
            return _context.Options.Any(e => e.Id == id);
        }

		private async Task<Option> GetExistingOption(Guid id)
		{
			var existingOption = await _context.Options
			.AsNoTracking()
			.FirstOrDefaultAsync(o => o.Id == id);

			return existingOption;
		}
	}
}

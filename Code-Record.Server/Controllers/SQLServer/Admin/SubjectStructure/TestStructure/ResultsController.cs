using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Code_Record.Server.Contexts;
using Code_Record.Server.Models.SQLServer.SubjectStructure.TestStructure;

namespace Code_Record.Server.Controllers.SQLServer.Admin.SubjectStructure.TestStructure
{
    [Route("api/sql/admin/[controller]")]
    [ApiController]
    public class ResultsController : ControllerBase
    {
        private readonly SQLServerContext _context;

        public ResultsController(SQLServerContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Result>>> GetResults()
        {
            return await _context.Results.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Result>> GetResult(Guid id)
        {
            var result = await _context.Results.FindAsync(id);

            if (result == null) return NotFound();

			return result;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutResult(Guid id, Result result)
        {
            if (id != result.Id) return BadRequest();
            var existingResult = GetExistingResult(id).Result;
            result.UploadDate = existingResult.UploadDate;
			_context.Entry(result).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ResultExists(id)) return NotFound();
				else throw;
			}

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteResult(Guid id)
        {
            var result = await _context.Results.FindAsync(id);
            if (result == null) return NotFound();

			_context.Results.Remove(result);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ResultExists(Guid id)
        {
            return _context.Results.Any(e => e.Id == id);
        }

		private async Task<Result> GetExistingResult(Guid id)
		{
			var existingResult = await _context.Results
			.AsNoTracking()
			.FirstOrDefaultAsync(r => r.Id == id);

			return existingResult;
		}
	}
}

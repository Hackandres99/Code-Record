using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Code_Record.Server.Contexts;
using Code_Record.Server.Models.SQLServer;

namespace Code_Record.Server.Controllers.SQLServer.Admin
{
    [Route("api/sql/admin/[controller]")]
    [ApiController]
    public class VisitsController : ControllerBase
    {
        private readonly SQLServerContext _context;

        public VisitsController(SQLServerContext context)
        {
            _context = context;
        }

		[HttpGet]
        public async Task<ActionResult<IEnumerable<Visit>>> GetVisits()
        {
            return await _context.Visits.ToListAsync();
        }

		[HttpGet("{id}")]
        public async Task<ActionResult<Visit>> GetVisit(Guid id)
        {
			var visit = await _context.Visits.FindAsync(id);

            if (visit == null) return NotFound();

			return visit;
        }

		[HttpPut("{id}")]
        public async Task<IActionResult> PutVisit(Guid id, Visit visit)
        {
            if (id != visit.Id) return BadRequest();

            var existingVisit = GetExistingVisit(id).Result;
            visit.VisitDate = existingVisit.VisitDate;
			_context.Entry(visit).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VisitExists(id)) return NotFound();
				else throw;
			}

            return NoContent();
        }

		[HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVisit(Guid id)
        {
            var visit = await _context.Visits.FindAsync(id);
            if (visit == null) return NotFound();

			_context.Visits.Remove(visit);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool VisitExists(Guid id)
        {
            return _context.Visits.Any(e => e.Id == id);
        }

		private async Task<Visit> GetExistingVisit(Guid id)
		{
			var existingVisit = await _context.Visits
			.AsNoTracking()
			.FirstOrDefaultAsync(v => v.Id == id);

			return existingVisit;
		}
	}
}

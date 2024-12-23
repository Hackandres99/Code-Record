using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Code_Record.Server.Contexts;
using Code_Record.Server.Models.SQLServer.SubjectStructure;
using Code_Record.Server.Models.SQLServer.DTOs;
using Code_Record.Server.Extensions.Controllers.DTOs;
using System.Security.Claims;

namespace Code_Record.Server.Controllers.SQLServer.Admin.SubjectStructure
{
    [Route("api/sql/admin/[controller]")]
    [ApiController]
    public class SubjectsController : ControllerBase
    {
        private readonly SQLServerContext _context;

        public SubjectsController(SQLServerContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SubjectDTO>>> GetSubjects()
        {
			return await _context.Subjects.ToSubjectDto("").ToListAsync();
		}

        [HttpGet("{id}")]
        public async Task<ActionResult<SubjectDTO>> GetSubject(Guid id)
        {
			var subject = await _context.Subjects
				.ToSubjectDto("")
				.FirstOrDefaultAsync(s => s.Id == id);

			if (subject == null) return NotFound();

			return subject;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutSubject(Guid id, Subject subject)
        {
            if (id != subject.Id) return BadRequest();
            var existingSubject = GetExistingSubject(id).Result;
            subject.CreationDate = existingSubject.CreationDate;
            subject.UpdateDate = DateTime.UtcNow;
			_context.Entry(subject).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SubjectExists(id)) return NotFound();
				else throw;
			}

            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<Subject>> PostSubject(Subject subject)
        {
            subject.CreationDate = DateTime.UtcNow;
            subject.UpdateDate = null;
            _context.Subjects.Add(subject);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetSubject", new { id = subject.Id }, subject);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSubject(Guid id)
        {
            var subject = await _context.Subjects.FindAsync(id);
            if (subject == null) return NotFound();

			_context.Subjects.Remove(subject);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool SubjectExists(Guid id)
        {
            return _context.Subjects.Any(e => e.Id == id);
        }

		private async Task<Subject> GetExistingSubject(Guid id)
		{
			var existingSubject = await _context.Subjects
			.AsNoTracking()
			.FirstOrDefaultAsync(s => s.Id == id);

			return existingSubject;
		}
	}
}

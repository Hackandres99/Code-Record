using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Code_Record.Server.Contexts;
using Code_Record.Server.Models.SQLServer.SubjectStructure.TestStructure;

namespace Code_Record.Server.Controllers.SQLServer.Admin.SubjectStructure.TestStructure
{
    [Route("api/sql/admin/[controller]")]
    [ApiController]
    public class TestsController : ControllerBase
    {
        private readonly SQLServerContext _context;

        public TestsController(SQLServerContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Test>>> GetTests()
        {
            return await _context.Tests.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Test>> GetTest(Guid id)
        {
            var test = await _context.Tests.FindAsync(id);

            if (test == null) return NotFound();

			return test;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutTest(Guid id, Test test)
        {
            if (id != test.Id) return BadRequest();
            var existingTest = GetExistingTest(id).Result;
            test.CreationDate = existingTest.CreationDate;
            test.UpdateDate = DateTime.UtcNow;
			_context.Entry(test).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TestExists(id)) return NotFound();
				else throw;
			}

            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<Test>> PostTest(Test test)
        {
            test.CreationDate = DateTime.Now;
            test.UpdateDate = null;
            _context.Tests.Add(test);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTest", new { id = test.Id }, test);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTest(Guid id)
        {
            var test = await _context.Tests.FindAsync(id);
            if (test == null) return NotFound();

			_context.Tests.Remove(test);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TestExists(Guid id)
        {
            return _context.Tests.Any(e => e.Id == id);
        }

		private async Task<Test> GetExistingTest(Guid id)
		{
			var existingTest = await _context.Tests
			.AsNoTracking()
			.FirstOrDefaultAsync(t => t.Id == id);

			return existingTest;
		}
	}
}

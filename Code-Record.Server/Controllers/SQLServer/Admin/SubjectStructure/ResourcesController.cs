using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Code_Record.Server.Contexts;
using Code_Record.Server.Models.SQLServer.SubjectStructure;

namespace Code_Record.Server.Controllers.SQLServer.Admin.SubjectStructure
{
    [Route("api/sql/admin/[controller]")]
    [ApiController]
    public class ResourcesController : ControllerBase
    {
        private readonly SQLServerContext _context;

        public ResourcesController(SQLServerContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Resource>>> GetResources()
        {
            return await _context.Resources.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Resource>> GetResource(Guid id)
        {
            var resource = await _context.Resources.FindAsync(id);

            if (resource == null) return NotFound();

			return resource;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutResource(Guid id, Resource resource)
        {
            if (id != resource.Id) return BadRequest();
            var existingResource = GetExistingResource(id).Result;
            resource.UploadDate = existingResource.UploadDate;
            resource.UpdateDate = DateTime.UtcNow;
			_context.Entry(resource).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ResourceExists(id)) return NotFound();
				else throw;
			}

            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<Resource>> PostResource(Resource resource)
        {
            resource.UploadDate = DateTime.UtcNow;
            resource.UpdateDate = null;
            _context.Resources.Add(resource);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetResource", new { id = resource.Id }, resource);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteResource(Guid id)
        {
            var resource = await _context.Resources.FindAsync(id);
            if (resource == null) return NotFound();

			_context.Resources.Remove(resource);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ResourceExists(Guid id)
        {
            return _context.Resources.Any(e => e.Id == id);
        }

		private async Task<Resource> GetExistingResource(Guid id)
		{
			var existingResource = await _context.Resources
			.AsNoTracking()
			.FirstOrDefaultAsync(r => r.Id == id);

			return existingResource;
		}
	}
}

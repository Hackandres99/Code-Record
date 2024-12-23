using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Code_Record.Server.Contexts;
using Code_Record.Server.Models.SQLServer;
using Code_Record.Server.Extensions.Controllers.DTOs;

namespace Code_Record.Server.Controllers.SQLServer.Admin
{
    [Route("api/sql/admin/[controller]")]
    [ApiController]
    public class AllowsController : ControllerBase
    {
        private readonly SQLServerContext _context;

        public AllowsController(SQLServerContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Allow>>> GetAllows()
        {
            return await _context.Allows.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Allow>> GetAllow(Guid id)
        {
            var allow = await _context.Allows.FindAsync(id);

            if (allow == null) return NotFound();

			return allow;
        }

		[HttpPost]
		public async Task<ActionResult<Allow>> PostAllow(Allow allow)
		{
			if (allow == null) return BadRequest(new { error = "Allow fields are required" });

			if (AllowExists(allow)) return Conflict(new { error = "The allow already exists." });

			allow.CreationDate = DateTime.UtcNow;
			allow.UpdateDate = null;

			_context.Allows.Add(allow);

			var subjectDTO = await _context.Subjects
				.Where(s => s.Id == allow.ResourceId)
				.ToSubjectDto("")
				.FirstOrDefaultAsync();

			if (subjectDTO == null) return NotFound("Subject not found.");

			AddPermissions(subjectDTO.Resources, "resource", allow.UserId);
			AddPermissions(subjectDTO.Tests.Select(t => t.Id), "test", allow.UserId);
			AddPermissions(subjectDTO.Themes.Select(t => t.Id), "theme", allow.UserId);

			ProcessNestedPermissions(
				subjectDTO.Tests,
				test => test.Questions.Select(q => q.Id),
				"question",
				allow.UserId
			);

			ProcessNestedPermissions(
				subjectDTO.Tests.SelectMany(test => test.Questions),
				question => question.Options,
				"option",
				allow.UserId
			);

			ProcessNestedPermissions(
				subjectDTO.Themes,
				theme => theme.Videos.Select(v => v.Id),
				"video",
				allow.UserId
			);

			await _context.SaveChangesAsync();
			return CreatedAtAction("GetAllow", new { id = allow.Id }, allow);
		}

		private void AddPermissions(IEnumerable<Guid> resourceIds, string resourceType, Guid userId)
		{
			foreach (var resourceId in resourceIds)
			{
				var permission = new Allow
				{
					UserId = userId,
					ResourceId = resourceId,
					ResourceType = resourceType,
					CreationDate = DateTime.UtcNow,
					UpdateDate = null
				};

				_context.Allows.Add(permission);
			}
		}

		private void ProcessNestedPermissions<T>(
			IEnumerable<T> parents,
			Func<T, IEnumerable<Guid>> childSelector,
			string childResourceType,
			Guid userId)
		{
			foreach (var parent in parents)
			{
				var childResourceIds = childSelector(parent);
				AddPermissions(childResourceIds, childResourceType, userId);
			}
		}

		[HttpDelete("{subjectId}/user/{userId}")]
		public async Task<IActionResult> DeletePermissions(Guid subjectId, Guid userId)
		{
			var subjectDTO = await _context.Subjects
				.Where(s => s.Id == subjectId)
				.ToSubjectDto("")
				.FirstOrDefaultAsync();

			if (subjectDTO == null)
				return NotFound(new { error = "Subject not found." });

			var resourceIdsToDelete = new HashSet<Guid> { subjectId };

			resourceIdsToDelete.UnionWith(subjectDTO.Resources);
			resourceIdsToDelete.UnionWith(subjectDTO.Tests.Select(t => t.Id));
			resourceIdsToDelete.UnionWith(subjectDTO.Tests.SelectMany(t => t.Questions.Select(q => q.Id)));
			resourceIdsToDelete.UnionWith(subjectDTO.Tests.SelectMany(t => t.Questions.SelectMany(q => q.Options)));
			resourceIdsToDelete.UnionWith(subjectDTO.Themes.Select(t => t.Id));
			resourceIdsToDelete.UnionWith(subjectDTO.Themes.SelectMany(t => t.Videos.Select(v => v.Id)));

			var allowsToDelete = await _context.Allows
				.Where(a => a.UserId == userId && resourceIdsToDelete.Contains(a.ResourceId))
				.ToListAsync();

			if (!allowsToDelete.Any())
				return NotFound(new { error = "No allows found for the given Subject and User." });

			_context.Allows.RemoveRange(allowsToDelete);
			await _context.SaveChangesAsync();

			return NoContent();
		}

		private bool AllowExists(Allow allow)
        {
            return _context.Allows.Any(a =>
				a.UserId == allow.UserId &&
				a.ResourceId == allow.ResourceId &&
				a.ResourceType == "Subject");
		}
	}
}

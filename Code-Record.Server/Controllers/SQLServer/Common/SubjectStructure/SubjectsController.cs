using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Code_Record.Server.Contexts;
using System.Security.Claims;
using Code_Record.Server.Models.SQLServer.DTOs;
using Code_Record.Server.Extensions.Controllers.DTOs;

namespace Code_Record.Server.Controllers.SQLServer.Common.SubjectStructure
{
    [Route("api/sql/common/[controller]")]
    [ApiController]
    public class SubjectsController : ControllerBase
    {
        private readonly SQLServerContext _context;

        public SubjectsController(SQLServerContext context)
        {
            _context = context;
        }

        [HttpGet("me")]
        public async Task<ActionResult<IEnumerable<SubjectDTO>>> GetSubjects()
        {
			var (userId, email) = GetCurrentUserInfo();
			var subjects = await _context.Subjects
				.Where(s => _context.Allows
					.Any(a => a.UserId == userId &&
							  a.ResourceId == s.Id &&
							  a.ResourceType == "subject"))
				.ToSubjectDto(email)
				.ToListAsync();

			return Ok(subjects);
		}

		[HttpGet("me/{id}")]
		public async Task<ActionResult<SubjectDTO>> GetSubject(Guid id)
		{

			var (userId, email) = GetCurrentUserInfo();
			var subject = await _context.Subjects
				.Where(s => s.Id == id && _context.Allows
					.Any(a => a.UserId == userId &&
							  a.ResourceId == id &&
							  a.ResourceType == "subject"))
				.ToSubjectDto(email)
				.FirstOrDefaultAsync();

			if (subject == null)
				return Forbid();

			return Ok(subject);
		}

		private (Guid UserId, string Email) GetCurrentUserInfo()
		{
			var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
				throw new UnauthorizedAccessException("User ID is not valid or is missing.");

			var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
			if (string.IsNullOrEmpty(email))
				throw new UnauthorizedAccessException("User email is not available or the user is not authenticated.");

			return (userId, email);
		}
	}
}

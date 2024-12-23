using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Code_Record.Server.Contexts;
using Code_Record.Server.Models.SQLServer.SubjectStructure.TestStructure;

namespace Code_Record.Server.Controllers.SQLServer.Common.SubjectStructure.TestStructure
{
    [Route("api/sql/common/[controller]")]
    [ApiController]
    public class ResultsController : ControllerBase
    {
        private readonly SQLServerContext _context;

        public ResultsController(SQLServerContext context)
        {
            _context = context;
        }

        [HttpGet("me")]
        public async Task<ActionResult<IEnumerable<Result>>> GetResults()
        {
			var userEmail = GetCurrentUserEmail();
			var results = await _context.Results.Where(r => r.UserEmail == userEmail).ToListAsync();

			return Ok(results);
		}

        [HttpGet("me/{id}")]
        public async Task<ActionResult<Result>> GetResult(Guid id)
        {
			var userEmail = GetCurrentUserEmail();
			var result = await _context.Results.FirstOrDefaultAsync(r => r.Id == id && r.UserEmail == userEmail);

			if (result == null)
				return NotFound(new { error = "Result not found or you do not have access." });

			return Ok(result);
		}

        [HttpPut("me/{id}")]
        public async Task<IActionResult> PutResult(Guid id, Result result)
        {
			var userEmail = GetCurrentUserEmail();

			if (id != result.Id) 
				return BadRequest(new { error = "Result ID does not match." });
			var existingResult = GetExistingResult(id, userEmail).Result;

			if (existingResult == null)
				return NotFound(new { error = "Result not found or you do not have access." });

			result.UploadDate = existingResult.UploadDate;
			_context.Entry(result).State = EntityState.Modified;

			try
			{
				await _context.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!ResultExists(id)) 
					return NotFound(new { error = "Result no longer exists." });
				else throw;
			}

			return NoContent();
		}

        [HttpPost]
        public async Task<ActionResult<Result>> PostResult(Result result)
        {
			result.UploadDate = DateTime.UtcNow;
            _context.Results.Add(result);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetResult", new { id = result.Id }, result);
        }

        private bool ResultExists(Guid id)
        {
            return _context.Results.Any(e => e.Id == id);
        }

		private string GetCurrentUserEmail()
		{
			var emailClaim = User.FindFirst(ClaimTypes.Email)?.Value;

			if (string.IsNullOrEmpty(emailClaim))
				throw new UnauthorizedAccessException("Email not found in claims.");

			return emailClaim;
		}

		private async Task<Result> GetExistingResult(Guid id, string userEmail)
		{
			var existingResult = await _context.Results
			.AsNoTracking()
			.FirstOrDefaultAsync(r => r.Id == id && r.UserEmail == userEmail);

			return existingResult;
		}
	}
}

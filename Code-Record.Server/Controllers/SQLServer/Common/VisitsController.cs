using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Code_Record.Server.Contexts;
using Code_Record.Server.Models.SQLServer;
using System.Security.Claims;

namespace Code_Record.Server.Controllers.SQLServer.Common
{
    [Route("api/sql/common/[controller]")]
    [ApiController]
    public class VisitsController : ControllerBase
    {
        private readonly SQLServerContext _context;

        public VisitsController(SQLServerContext context)
        {
            _context = context;
        }

        [HttpGet("count")]
        public async Task<ActionResult<int>> GetVisits()
        {
            return await _context.Visits.CountAsync();
        }

        [HttpPost]
        public async Task<ActionResult<Visit>> PostVisit()
        {
			var visit = new Visit
			{
				VisitDate = DateTime.Now,
				UserEmail = GetCurrentUserEmail()
			};

			_context.Visits.Add(visit);
			await _context.SaveChangesAsync();

			return CreatedAtAction("GetVisit", new { id = visit.Id }, visit);
		}

		private string GetCurrentUserEmail()
		{
			return User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value
				   ?? throw new UnauthorizedAccessException("User is not authenticated.");
		}
	}
}

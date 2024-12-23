using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace Code_Record.Server.Extensions.Controllers
{
	public class AdminAuthorize: IAuthorizationFilter
	{
		private readonly string _requiredRole;
		private readonly string[] _protectedRoutes;

		public AdminAuthorize(string requiredRole, params string[] protectedRoutes)
		{
			_requiredRole = requiredRole;
			_protectedRoutes = protectedRoutes;
		}

		public void OnAuthorization(AuthorizationFilterContext context)
		{
			var route = context.HttpContext.Request.Path.Value;

			if (route != null && _protectedRoutes.Any(r => route.StartsWith(r, StringComparison.OrdinalIgnoreCase)))
			{
				if (!context.HttpContext.User.Identity.IsAuthenticated ||
					!context.HttpContext.User.IsInRole(_requiredRole))
				{
					context.Result = new ForbidResult();
				}
			}
		}
	}
}

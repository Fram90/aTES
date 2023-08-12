using Microsoft.AspNetCore.Mvc.Filters;

namespace aTES.TaskTracker.Infrastructure;

public class MustHaveAnyRoleAttribute : ActionFilterAttribute
{
    private readonly string[] _allowedRoles;

    /// <summary>
    /// Allowed roles separated by comma "Worker, Manager"
    /// </summary>
    /// <param name="allowedRoles"></param>
    public MustHaveAnyRoleAttribute(string allowedRoles)
    {
        _allowedRoles = allowedRoles.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
    }
    
    public MustHaveAnyRoleAttribute(params string[] allowedRoles)
    {
        _allowedRoles = allowedRoles;
    }

    public override Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var authorized = _allowedRoles.Any(role => context.HttpContext.User.IsInRole(role));
        if (!authorized)
        {
            context.HttpContext.Response.StatusCode = 403;
            return Task.CompletedTask;
        }

        return next();
    }
}
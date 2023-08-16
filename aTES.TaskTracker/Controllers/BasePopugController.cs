using System.Security.Claims;
using aTES.TaskTracker.Domain;
using aTES.TaskTracker.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace aTES.TaskTracker.Controllers;

public abstract class BasePopugController : ControllerBase
{
    protected PopugIdentity GetCurrentUser()
    {
        var publicId = User.Claims.FirstOrDefault(x => x.Type == AuthConsts.CLAIMS_PUBLIC_ID)?.Value;
        if (publicId == null)
            throw new Exception("No PublicId in user claims");

        var role = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role);
        if (publicId == null)
            throw new Exception("No Role in user claims");

        var user = new PopugIdentity()
        {
            PublicId = Guid.Parse(publicId),
            Role = role!.Value
        };

        return user;
    }
}
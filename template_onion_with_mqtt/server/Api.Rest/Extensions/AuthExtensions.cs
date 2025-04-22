using System.Security.Authentication;

namespace Api.Rest.Extensions;

public static class AuthExtensions
{
    public static string GetJwt(this HttpContext ctx)
    {
        return ctx.Request.Headers.Authorization.FirstOrDefault() ??
               throw new AuthenticationException("No token provided");
    }
}
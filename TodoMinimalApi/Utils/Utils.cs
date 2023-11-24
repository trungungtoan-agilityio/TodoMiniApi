using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;

namespace TodoMinimalApi.Utils;

public static class Utils
{
    public static string GetUserEndPoint(HttpContext context)
    {
        var tokenValue = string.Empty;
        if (AuthenticationHeaderValue.TryParse(context.Request.Headers["Authorization"], out var authHeader))
        {
            tokenValue = authHeader.Parameter;
        }
        var email = "";
        if (!string.IsNullOrEmpty(tokenValue))
        {
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(tokenValue);
            email = token.Claims.First(claim => claim.Type == "Email").Value;
        }

        return $"User {email ?? "Anonymous"} endpoint:{context.Request.Path}"
               + $" {context.Connection.RemoteIpAddress}";
    }
}
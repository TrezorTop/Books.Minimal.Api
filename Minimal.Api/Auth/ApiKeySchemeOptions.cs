using Microsoft.AspNetCore.Authentication;

namespace Minimal.Api.Auth;

public class ApiKeySchemeOptions : AuthenticationSchemeOptions
{
    public string ApiKey { get; set; } = "SecretKey";
}
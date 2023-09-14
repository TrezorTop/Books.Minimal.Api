using Microsoft.AspNetCore.Authentication;

namespace Minimal.Api.Auth;

public abstract class ApiKeySchemeConstants : AuthenticationSchemeOptions
{
    public const string SchemeName = "ApiKeyScheme";
}
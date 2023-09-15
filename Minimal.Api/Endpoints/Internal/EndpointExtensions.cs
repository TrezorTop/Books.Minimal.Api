using System.Reflection;

namespace Minimal.Api.Endpoints.Internal;

public static class EndpointExtensions
{
    public static void AddEndpointsServices<TMarker>(this IServiceCollection services, IConfiguration configuration)
    {
        var endpointTypes = GetEndpointsTypesFromAssemblyContaining(typeof(TMarker));

        foreach (var endpointType in endpointTypes)
        {
            endpointType.GetMethod(nameof(IEndpoints.AddServices))!.Invoke(null, new object[] { services, configuration });
        }
    }

    public static void UseEndpoints<TMarker>(this IApplicationBuilder app)
    {
        var endpointTypes = GetEndpointsTypesFromAssemblyContaining(typeof(TMarker));

        foreach (var endpointType in endpointTypes)
        {
            endpointType.GetMethod(nameof(IEndpoints.DefineEndpoints))!.Invoke(null, new object[] { app });
        }
    }

    private static IEnumerable<TypeInfo> GetEndpointsTypesFromAssemblyContaining(Type typeMarker)
    {
        return typeMarker.Assembly.DefinedTypes.Where(
            x => x is { IsAbstract: false, IsInterface: false } && typeof(IEndpoints).IsAssignableFrom(x)
        );
    }
}
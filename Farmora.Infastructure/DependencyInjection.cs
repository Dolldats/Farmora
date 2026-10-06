using Farmora.Application.Users;
using Farmora.Infastructure.Users;
using Microsoft.Extensions.DependencyInjection;

namespace Farmora.Infastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        return services;
    }
}

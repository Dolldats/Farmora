using Farmora.Application.Interfaces.Repositories;
using Farmora.Infastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Farmora.Infastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<Farmora.Application.Interfaces.Services.IUserService, Services.UserService>();
        return services;
    }
}

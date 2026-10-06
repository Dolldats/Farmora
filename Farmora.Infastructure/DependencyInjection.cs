using Farmora.Application.Interfaces.Repositories;
using Farmora.Application.Interfaces.Services;
using Farmora.Infastructure.Repositories;
using Farmora.Infastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Farmora.Domain.Entities;

namespace Farmora.Infastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        return services;
    }
}

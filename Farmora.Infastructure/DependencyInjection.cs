using Farmora.Application.Interfaces.Repositories;
using Farmora.Application.Interfaces.Services;
using Farmora.Infastructure.Identity;
using Farmora.Infastructure.Repositories;
using Farmora.Infastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Farmora.Domain.Entities;

namespace Farmora.Infastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration? configuration = null)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

        if (configuration is not null)
        {
            services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
            services.Configure<AdminSeedOptions>(configuration.GetSection("AdminSeed"));
        }
        else
        {
            services.Configure<JwtOptions>(_ => { });
            services.Configure<AdminSeedOptions>(_ => { });
        }
        services.AddSingleton<JwtTokenService>();
        services.AddHttpContextAccessor();

        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddScoped<IReportRepository, ReportRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IVerificationRepository, VerificationRepository>();
        services.AddScoped<IConversationRepository, MessagingRepository>();
        services.AddScoped<IMessageRepository, MessagingRepository>();
        services.AddScoped<IFollowRepository, FollowRepository>();
        services.AddScoped<IAnalyticsRepository, AnalyticsRepository>();
        services.AddScoped<IDashboardRepository, AnalyticsRepository>();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IFileService>(_ => new FileService());
        services.AddScoped<IReviewService, ReviewService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IVerificationService, VerificationService>();
        services.AddScoped<IMessagingService, MessagingService>();
        services.AddScoped<IFollowService, FollowService>();
        services.AddScoped<IAnalyticsService, AnalyticsService>();
        services.AddScoped<IDashboardService, AnalyticsService>();

        return services;
    }
}

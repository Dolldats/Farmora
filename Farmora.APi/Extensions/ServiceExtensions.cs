using System.Text;
using Farmora.Application.Common;
using Farmora.Application.Interfaces.Repositories;
using Farmora.Application.Interfaces.Services;
using Farmora.APi.Infrastructure;
using Farmora.Infastructure.Data;
using Farmora.Infastructure.Identity;
using Farmora.Infastructure.Repositories;
using Farmora.Infastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

namespace Farmora.APi.Extensions;

public static class ServiceExtensions
{
    public const string CorsPolicy = "FarmoraCors";

    public static IServiceCollection AddCurrentUser(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, HttpCurrentUser>();
        return services;
    }

    /// <summary>Fill gaps left by Infrastructure DI so controllers can resolve every service.</summary>
    public static IServiceCollection AddFarmoraApiServices(this IServiceCollection services)
    {
        services.TryAddScoped<IAddressRepository, AddressRepository>();
        services.TryAddScoped<ICartRepository, CartRepository>();
        services.TryAddScoped<ICategoryRepository, CategoryRepository>();
        services.TryAddScoped<IOrderRepository, OrderRepository>();
        services.TryAddScoped<IPaymentRepository, PaymentRepository>();
        services.TryAddScoped<IDeliveryRepository, DeliveryRepository>();
        services.TryAddScoped<IPromotionRepository, PromotionRepository>();
        services.TryAddScoped<IAuthRepository, AuthRepository>();
        services.TryAddScoped<IAuditRepository, AuditRepository>();
        services.TryAddScoped<ICustomerRepository, CustomerRepository>();
        services.TryAddScoped<IFarmerRepository, FarmerRepository>();
        services.TryAddScoped<IRoleRepository, RoleRepository>();
        services.TryAddScoped<IProductRepository, ProductRepository>();
        services.TryAddScoped<IProductImageRepository, ProductImageRepository>();

        services.TryAddScoped<IAddressService, AddressService>();
        services.TryAddScoped<ICartService, CartService>();
        services.TryAddScoped<IOrderService, OrderService>();
        services.TryAddScoped<IPaymentService, PaymentService>();
        services.TryAddScoped<IDeliveryService, DeliveryService>();
        services.TryAddScoped<IPromotionService, PromotionService>();
        services.TryAddScoped<IWishlistService, WishlistService>();

        return services;
    }

    public static IServiceCollection AddFarmoraAuth(this IServiceCollection services, IConfiguration configuration)
    {
        var jwt = configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();
        if (string.IsNullOrWhiteSpace(jwt.Secret) || jwt.Secret.Length < 32)
            throw new InvalidOperationException("Jwt:Secret must be configured with at least 32 characters.");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Secret));

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwt.Issuer,
                ValidAudience = jwt.Audience,
                IssuerSigningKey = key,
                ClockSkew = TimeSpan.Zero,
            };
            options.Events = new JwtBearerEvents
            {
                // Security-stamp / verification checks are enforced in services; accept the token here.
                OnTokenValidated = _ => Task.CompletedTask,
            };
        });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("CustomerOnly", p => p.RequireRole("Customer", "Admin"));
            options.AddPolicy("FarmerOnly", p => p.RequireRole("Farmer", "Admin"));
            // Verification state is enforced in services; policy only requires the Farmer role.
            options.AddPolicy("VerifiedFarmer", p => p.RequireRole("Farmer", "Admin"));
            options.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
            options.AddPolicy("ResourceOwnerOrAdmin", p => p.RequireAuthenticatedUser());
        });

        return services;
    }

    public static IServiceCollection AddFarmoraCors(this IServiceCollection services, IConfiguration configuration)
    {
        var origins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? ["http://localhost:3000", "http://localhost:5173", "http://localhost:4200"];
        services.AddCors(options =>
        {
            options.AddPolicy(CorsPolicy, policy => policy
                .WithOrigins(origins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials());
        });
        return services;
    }

    public static IServiceCollection AddFarmoraHealth(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddDbContextCheck<FarmoraDbContext>(tags: ["ready"]);
        return services;
    }

    public static IServiceCollection AddSwaggerWithJwt(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "Farmora API", Version = "v1" });
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
            });
            options.AddSecurityRequirement(_ => new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecuritySchemeReference("Bearer"),
                    new List<string>()
                },
            });
        });
        return services;
    }
}

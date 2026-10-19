using Farmora.APi.Extensions;
using Farmora.APi.Middleware;
using Farmora.Infastructure;
using Farmora.Infastructure.Data;
using Farmora.Infastructure.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Dev-only Jwt defaults so local runs work without secrets.
if (string.IsNullOrWhiteSpace(builder.Configuration["Jwt:Secret"]))
{
    builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
    {
        ["Jwt:Issuer"] = builder.Configuration["Jwt:Issuer"] ?? "Farmora",
        ["Jwt:Audience"] = builder.Configuration["Jwt:Audience"] ?? "FarmoraApi",
        ["Jwt:Secret"] = "dev-only-secret-that-is-at-least-64-characters-long-0123456789abcdef",
        ["Jwt:AccessMinutes"] = builder.Configuration["Jwt:AccessMinutes"] ?? "15",
        ["Jwt:RefreshDays"] = builder.Configuration["Jwt:RefreshDays"] ?? "7",
    });
}

builder.Services.AddControllers();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddFarmoraApiServices();
builder.Services.AddCurrentUser();
builder.Services.AddFarmoraAuth(builder.Configuration);
builder.Services.AddFarmoraCors(builder.Configuration);
builder.Services.AddFarmoraHealth();
builder.Services.AddSwaggerWithJwt();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("DefaultConnection is not configured.");
builder.Services.AddDbContext<FarmoraDbContext>(options => options.UseNpgsql(connectionString));

var app = builder.Build();

app.UseExceptionMiddleware();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors(ServiceExtensions.CorsPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health/live", new() { Predicate = _ => false });
app.MapHealthChecks("/health/ready");

await AdminSeeder.SeedAsync(app.Services);

app.Run();

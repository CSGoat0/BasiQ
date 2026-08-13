using BasiQBLL.Services.Abstraction;
using BasiQBLL.Services.Implementation;
using BasiQBLL.Settings;
using BasiQBLL.Validation.Filters;
using BasiQDAL.Database;
using BasiQDAL.Entities;
using BasiQDAL.Repositories.Abstraction;
using BasiQDAL.Repositories.Implementation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace BasiQBLL.Helpers
{
    public static class ServiceExtensions
    {
        public static void AddBasiQIdentity(this IServiceCollection services, IConfiguration Configuration)
        {
            services.AddDataProtection();
            services.AddIdentityCore<User>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<BasiQDbContext>()
            .AddDefaultTokenProviders();
        }

        public static void AddBasiQConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
        }

        public static void AddBasiQEnhancedConnectionString(this IServiceCollection services, IConfiguration configuration, string stringName = "defaultConnection")
        {
            var connectionString = configuration.GetConnectionString(stringName);
            services.AddDbContext<BasiQDbContext>(options =>
                options.UseSqlServer(
                    connectionString,
                    b => b.MigrationsAssembly("BasiQDAL")
                    ));
            services.AddHealthChecks()
        .AddSqlServer(
            connectionString: connectionString,
            name: "BasiQ-DB",
            failureStatus: HealthStatus.Unhealthy,
            tags: new[] { "db", "sql", "BasiQ" }
        );
        }

        public static void AddBasiQDependencyInjection(this IServiceCollection services)
        {
            // Repositories
            services.AddScoped<IUserRepository, UserRepository>();

            // Services
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IEmailService, EmailService>();

            // Validation Filters
            services.AddScoped<EmailUniquenessFilter>();
        }

        public static void AddThirdPartyAuthentication(this IServiceCollection services, IConfiguration Configuration)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = Configuration["Jwt:Issuer"],
                ValidAudience = Configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                    Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured")))
            };
        })
        .AddCookie("ExternalCookie"); // dedicated scheme for OAuth handshake only
        }
    }
}

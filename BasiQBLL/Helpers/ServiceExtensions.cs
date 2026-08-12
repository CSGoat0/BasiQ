using BasiQDAL.Database;
using BasiQDAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using BasiQDAL.Repositories.Implementation;
using BasiQDAL.Repositories.Abstraction;
using BasiQBLL.Services.Abstraction;
using BasiQBLL.Services.Implementation;

namespace BasiQBLL.Helpers
{
    public static class ServiceExtensions
    {
        public static void BasiQIdentity(this IServiceCollection services, IConfiguration Configuration)
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

        public static void BasiQEnhancedConnectionString(this IServiceCollection services, IConfiguration configuration, string stringName = "defaultConnection")
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

        public static void BasiQDependencyInjection(this IServiceCollection services)
        {
            // Repositories
            services.AddScoped<IUserRepository, UserRepository>();

            // Services
            services.AddScoped<IUserService, UserService>();
        }
    }
}


using BasiQBLL.Extensions;
using Microsoft.OpenApi.Models;
using System.Reflection;

namespace BasiQPLL
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Configuration.AddEnvironmentVariables();
            builder.Services.AddBasiQEnhancedConnectionString(builder.Configuration);
            builder.Services.AddBasiQDependencyInjection();
            builder.Services.AddBasiQIdentity(builder.Configuration);
            builder.Services.AddBasiQConfiguration(builder.Configuration);
            builder.Services.AddThirdPartyAuthentication(builder.Configuration);

            // Add services to the container.
            builder.Services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                // This prevents unknown properties from crashing deserialization
            }); ;
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "BasiQ API",
                    Version = "v1"
                });

                // Load XML from TheBasiQPL (controllers)
                var plXmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var plXmlPath = Path.Combine(AppContext.BaseDirectory, plXmlFile);
                if (File.Exists(plXmlPath))
                {
                    c.IncludeXmlComments(plXmlPath);
                }

                // Load XML from TheBasiQBLL (DTOs)
                var bllXmlFile = "TheBasiQBLL.xml";
                var bllXmlPath = Path.Combine(AppContext.BaseDirectory, bllXmlFile);
                if (File.Exists(bllXmlPath))
                {
                    c.IncludeXmlComments(bllXmlPath);
                }
            });

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new Exception("DefaultConnection connection string is missing");
            }

            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.MapHealthChecks("/health");// check if the db connected or not

            // Configure the HTTP request pipeline.
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseDeveloperExceptionPage();

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseCors("AllowAngular");
            app.UseAuthentication();
            app.UseAuthorization();

            // map to swagger view as start view
            app.MapGet("/", () => Results.Redirect("/swagger"));

            app.MapControllers();

            app.Run();
        }
    }
}

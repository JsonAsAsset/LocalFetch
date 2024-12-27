using CUE4Parse.FileProvider.Vfs;
using Microsoft.EntityFrameworkCore;
using LocalFetchRestAPI.Controllers;

// Startup of Local Fetch's API
// The API is at http://localhost:1500
//
// If you know any better, remove the Main function from this, as it's not needed, but I'm not quite sure how to without compile errors

namespace LocalFetchRestAPI
{
    public class LocalFetchApi
    {
        public static AbstractVfsFileProvider? Provider;

        public LocalFetchApi(AbstractVfsFileProvider _provider)
        {
            Provider = _provider;
        }

        public async Task RunApi(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var services = builder.Services;

            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            builder.Logging.SetMinimumLevel(LogLevel.Information);
            builder.Logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.None);
            builder.Services.AddControllers().AddApplicationPart(typeof(LocalFetchApiController).Assembly);

            services.AddDbContext<DbContext>(opt => opt.UseInMemoryDatabase("LocalFetchWeb"));
            services.AddControllers();
            services.AddEndpointsApiExplorer();

            services.AddHsts(options =>
            {
                options.Preload = true;
                options.IncludeSubDomains = true;
                options.MaxAge = TimeSpan.FromDays(365);
            });

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseCors();
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            Console.WriteLine("Web API is running in the background.");

            await app.RunAsync("http://localhost:1500");
        }
        
        // Doesn't compile without this ?
        public static void Main(string[] args)
        {
        }
    }
}

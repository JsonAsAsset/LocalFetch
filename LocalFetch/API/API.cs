using CUE4Parse.FileProvider.Vfs;
using Microsoft.EntityFrameworkCore;

// Startup of Local Fetch's API

namespace LocalFetchRestAPI
{
    public class LocalFetchApi
    {
        public static AbstractVfsFileProvider? Provider;

        public LocalFetchApi(AbstractVfsFileProvider Newprovider)
        {
            Provider = Newprovider;
        }

        public static void Main(string[] args)
        {
        }

        public void Start(string[] args)
        {
            Task.Run(async () => await RunWebApp(args)).GetAwaiter().GetResult();
        }

        public async Task RunWebApp(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var services = builder.Services;

            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            builder.Logging.SetMinimumLevel(LogLevel.Information);
            builder.Logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.None);

            services.AddDbContext<DbContext>(opt => opt.UseInMemoryDatabase("LocalFetchWeb"));
            services.AddControllers();  // Ensure controllers are added
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

            await app.RunAsync();
        }
    }
}

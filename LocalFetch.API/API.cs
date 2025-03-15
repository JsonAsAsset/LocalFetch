using CUE4Parse.FileProvider.Vfs;
using Microsoft.EntityFrameworkCore;

using LocalFetch.API.Controllers;
using LocalFetch.Shared;

namespace LocalFetch.API;

/* Startup of Local Fetch's API */
public class LocalFetchApi
{
    public static AbstractVfsFileProvider? Provider;

    public LocalFetchApi(AbstractVfsFileProvider provider)
    {
        Provider = provider;
    }

    public async Task RunApi(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var services = builder.Services;

        builder.Services.AddControllers().AddApplicationPart(typeof(LocalFetchApiController).Assembly);
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        builder.Logging.SetMinimumLevel(LogLevel.Information);
        builder.Logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.None);

        services.AddDbContext<DbContext>(opt => opt.UseInMemoryDatabase("LocalFetch.API.Controllers"));
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
            options.AddDefaultPolicy(policyBuilder =>
            {
                policyBuilder.AllowAnyOrigin()
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

        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
        
        Console.WriteLine($"LocalFetch.API is running in the background: {Globals.LOCAL_FETCH_URL}");
        await app.RunAsync(Globals.LOCAL_FETCH_URL);
    }
}
using System;
using System.Linq;
using System.Runtime.InteropServices;
using LocalFetch.API.Controllers;
using LocalFetch.Shared;
using LocalFetch.Shared.Models;
using LocalFetch.Shared.Settings;
using LocalFetch.Shared.Settings.Builds;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

await Helper.LoadNativeLibraries(Globals.CacheFolder);

Logger.Log($"Local Fetch v{Globals.VersionString}");
Logger.Log($"Framework: {RuntimeInformation.FrameworkDescription}");

var Builds = Build.LoadAll();

var enumerable = Builds as Build[] ?? Builds.ToArray();
var usingBuilds = enumerable.Where(build => build.Name.Equals("6.21")).ToList();
usingBuilds.AddRange(enumerable.Where(build => build.Name.Equals("Latest")));

foreach (var build in enumerable)
{
    await build.Save();
}

foreach (var build in usingBuilds)
{
    await build.Initialize();
}

LocalFetchApiController.builds = usingBuilds;

Console.Title = "Local Fetch API";

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

builder.Services.AddControllers().AddApplicationPart(typeof(LocalFetchApiController).Assembly);
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

var URL = $"http://localhost:{UserSettings.Port}";

Logger.Log($"LocalFetch.API is running in the background: {URL}", LogType.Info);
await app.RunAsync(URL);

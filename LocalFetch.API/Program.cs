using LocalFetch.Utilities;
using Microsoft.EntityFrameworkCore;

Console.Title = "Local Fetch";

Console.Clear();

/* Builder ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ */
var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);
builder.Logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.None);

// Create DB LocalFetch
services.AddSingleton<FetchContext>();
services.AddDbContext<DbContext>(opt => opt.UseInMemoryDatabase("LocalFetch"));
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

// Initialize globals and log the initialization
var globals = app.Services.GetRequiredService<FetchContext>();
await globals.Initialize();

// Log creation of provider
Logger.Log("Initialized provider successfully");

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

/* Completed :] */
Logger.Log("Running API...");
Logger.Log("Please keep this window open while using JsonAsAsset.", LogType.Info);
Logger.Log("Developed by Tector and in collaboration with GMatrixGames. Thank you for using our software!", LogType.Credits);

app.Run();
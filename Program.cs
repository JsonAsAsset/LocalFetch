using Microsoft.EntityFrameworkCore;

Console.Title = "Local Fetch";

// Create Builder and services
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
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();

// Initialize globals and log the initialization
var globals = app.Services.GetRequiredService<FetchContext>();
await globals.Initialize();

// Log creation of provider
globals.WriteLog("CORE", ConsoleColor.Cyan, "Initialized provider successfully");

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

// Completed :]
globals.WriteLog("CORE", ConsoleColor.Cyan, "Running API...");
globals.WriteLog("INFO", ConsoleColor.Yellow, "Don't close this window unless you are done using JsonAsAsset, it will not work without it.");
globals.WriteLog("CREDITS", ConsoleColor.Blue, "Developed by Tector and GMatrix. Thank you for using our software!");

app.Run();
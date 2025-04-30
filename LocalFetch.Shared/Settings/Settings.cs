using System;
using System.IO;
using System.Text.Json;

namespace LocalFetch.Shared.Settings;

public sealed class UserSettings
{
#if DEBUG
    public static readonly string FilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "LocalFetch",
        "Settings_Debug.json");
#else
    public static readonly string FilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "LocalFetch",
        "Settings.json");
#endif
    
    /// <summary>
    /// Port to host the API at, default 1500
    /// </summary>
    public const int Port = 1500;

    public static UserSettings Load()
    {
        try
        {
            if (!File.Exists(FilePath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);

                var newJson = JsonSerializer.Serialize(new UserSettings(), new JsonSerializerOptions { WriteIndented = true });
            
                File.WriteAllText(FilePath, newJson);
            }

            var json = File.ReadAllText(FilePath);
            
            return JsonSerializer.Deserialize<UserSettings>(json) ?? new UserSettings();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error loading settings: {ex.Message}");
            
            return new UserSettings();
        }
    }

    public void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);

            var json = JsonSerializer.Serialize(this, options: new JsonSerializerOptions { WriteIndented = true });
            
            File.WriteAllText(FilePath, json);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error saving settings: {ex.Message}");
        }
    }
}
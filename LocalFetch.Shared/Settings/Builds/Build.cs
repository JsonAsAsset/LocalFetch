using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using CUE4Parse.MappingsProvider;
using CUE4Parse.UE4.Versions;
using LocalFetch.Shared.Models;
using LocalFetch.Shared.Settings.Builds.Containers;
using LocalFetch.Shared.Settings.Builds.Containers.Aes;
using SystemPath = System.IO.Path;

namespace LocalFetch.Shared.Settings.Builds;

/// <summary>
/// Individual Build Settings Class saved at Roaming/LocalFetch/Builds
/// </summary>
public sealed class Build
{
    public string Name { get; set; } = "Build";
    public string Path { get; set; } = string.Empty;
    public string MappingsFilePath { get; set; } = string.Empty;
    public EGame Version { get; set; } = EGame.GAME_UE5_LATEST;
    public AesContainer Aes { get; set; } = new();
    public BuildDisplayContainer Display { get; set; } = new();
    
    /* If used as a secondary build, always grab these asset types from this build only */
    public List<string> SecondaryAssetTypes { get; set; } = [];
    
    [JsonIgnore] public string FilePath => SystemPath.Combine(Globals.BuildsFolder.ToString(), $"{SanitizeFileName(Name)}.json");
    [JsonIgnore] public string VersionDisplay => Version.ToString();
    
    /* ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ */

    /* Helper method to sanitize file names */
    private static string SanitizeFileName(string fileName)
    {
        var invalidChars = new string(SystemPath.GetInvalidFileNameChars());
        var regexSearch = $"[{Regex.Escape(invalidChars)}]";
        return Regex.Replace(fileName, regexSearch, "_").Replace(" ", "_");
    }
    
    /* Asynchronous save method */
    public async Task Save()
    {
        Directory.CreateDirectory(Globals.BuildsFolder.ToString());
        var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        
        await File.WriteAllTextAsync(FilePath, json);
    }
    
    public void Delete()
    {
        if (File.Exists(FilePath))
        {
            File.Delete(FilePath);
            
            Console.WriteLine($"Deleted build settings file: {FilePath}");
        }
        else
        {
            Console.Error.WriteLine($"Build settings file not found: {FilePath}");
        }
    }
    
    public static IEnumerable<Build> LoadAll()
    {
        var settingsList = new List<Build>();

        Directory.CreateDirectory(Globals.BuildsFolder.ToString());

        var buildFiles = Directory.GetFiles(Globals.BuildsFolder.ToString(), "*.json");
        foreach (var file in buildFiles)
        {
            try
            {
                var json = File.ReadAllText(file);
                var buildSetting = JsonSerializer.Deserialize<Build>(json);
                if (buildSetting != null)
                {
                    settingsList.Add(buildSetting);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error loading build settings from {file}: {ex.Message}");
            }
        }

        return settingsList;
    }
    
    private string GetAbbreviation()
    {
        var words = Name.Split(' ', StringSplitOptions.RemoveEmptyEntries).Where(word => word.Any(char.IsLetter)).ToArray();

        switch (words.Length)
        {
            case >= 2:
            {
                return $"{char.ToUpper(words[0][0])}{char.ToUpper(words[1][0])}";
            }

            case 1:
            {
                var word = words[0];
                if (word.Length < 2)
                {
                    return char.ToUpper(word[0]).ToString();
                }

                var mid = word.Length / 2;
                return $"{char.ToUpper(word[0])}{char.ToUpper(word[mid])}";
            }
            
            default: {
                return string.Empty;
            }
        }
    }
    
    [JsonIgnore]
    public string Abbreviation => GetAbbreviation();
    
    public Bitmap? LoadSplashBitmap()
    {
        var index = Path.IndexOf("Content", StringComparison.OrdinalIgnoreCase);
        if (index < 0)
        {
            return null;
        }

        var contentFolder = Path[..(index + "Content".Length)];
        var splashPath = SystemPath.Combine(contentFolder, "Splash", "Splash.bmp");

        if (!File.Exists(splashPath)) return null;
        try
        {
            return new Bitmap(splashPath);
        }
        catch (Exception)
        {
            return null;
        }
    }
    
    /* CUE4Parse behavior ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ */
    [JsonIgnore] public Bitmap? SplashBitmap => LoadSplashBitmap();
    [JsonIgnore] public bool SplashVisibility => SplashBitmap != null;
    [JsonIgnore] public bool IsInitialized;
    [JsonIgnore] public required BuildProvider Provider;
    
    private Task InitializeProvider()
    {
        if (Path.Length != 0)
        {
            Provider = new BuildProvider(Path, new VersionContainer(Version));
        }
        
        if (!Aes.IsValid) Aes.Key = Globals.EMPTY_CHAR;
        Provider.Initialize();
        
        return Task.CompletedTask;
    }

    private Task InitializeTextureStreaming()
    {
        return Task.CompletedTask;
    }
    
    private async Task LoadKeys()
    {
        await Provider.SubmitKeyAsync(Globals.EMPTY_GUID, Aes.EncryptionKey);
        Logger.Log($"Submitted AES Key: {Aes.EncryptionKey}", LogType.CUE4, Name);

        if (Aes.HasDynamicKeys)
        {
            foreach (var vfs in Provider.UnloadedVfs.ToArray())
            {
                foreach (var extraKey in Aes.DynamicKeys.Where(extraKey => !extraKey.IsInvalid).Where(extraKey => vfs.TestAesKey(extraKey.EncryptionKey)))
                {
                    await Provider.SubmitKeyAsync(vfs.EncryptionKeyGuid, extraKey.EncryptionKey);
                    Logger.Log($"Submitted Dynamic AES Key: {extraKey.EncryptionKey}", LogType.CUE4, Name);
                }
            }
        }
    }
    
    private Task LoadMappings()
    {
        if (string.IsNullOrEmpty(MappingsFilePath)) return Task.CompletedTask;
        
        Provider.MappingsContainer = new FileUsmapTypeMappingsProvider(MappingsFilePath);
        Logger.Log($"Loaded Mappings: {MappingsFilePath}", LogType.CUE4, Name);
        
        return Task.CompletedTask;
    }
    
    private Task LoadAssetRegistries()
    {
        return Task.CompletedTask;
    }

    public async Task Initialize()
    {
        Logger.Log($"Initializing build {Name}..", LogType.Info);
        
        await InitializeProvider();
        await InitializeTextureStreaming();
        
        await LoadKeys();
        Provider.LoadVirtualPaths();
        Provider.PostMount();

        await LoadMappings();
        await LoadAssetRegistries();
        
        IsInitialized = true;
        
        Logger.Log($"Initialized build {Name}", LogType.Info);
    }
}
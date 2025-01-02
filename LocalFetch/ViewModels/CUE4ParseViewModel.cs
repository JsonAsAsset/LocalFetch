using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia.Threading;
using CUE4Parse.Compression;
using CUE4Parse.Encryption.Aes;
using CUE4Parse.FileProvider;
using CUE4Parse.FileProvider.Vfs;
using CUE4Parse.MappingsProvider;
using CUE4Parse.UE4.Objects.Core.Misc;
using CUE4Parse.UE4.Versions;
using CUE4Parse.UE4.VirtualFileSystem;
using CUE4Parse.Utils;
using LocalFetch.Services;
using LocalFetch.Shared.Framework;
using UE4Config.Parsing;

namespace LocalFetch.ViewModels;

// ReSharper disable once InconsistentNaming
public class CUE4ParseViewModel : ViewModelBase
{
    private string _internalGameName;
    public string InternalGameName
    {
        get => _internalGameName;
        set => SetProperty(ref _internalGameName, value);
    }
    
    public AbstractVfsFileProvider? Provider;
    
    public static string? MappingFilePath;
    public static string? ArchiveDirectory;
    public static EGame UnrealVersion;
    public static string? ArchiveKey;

    public void WriteLog(string source, ConsoleColor color, string description)
    {
        Console.ForegroundColor = color;
        Console.Write('[' + source + "] ");
        Console.ResetColor();
        Console.WriteLine(description);
    }

    public string GetStringProperty(ConfigIni config, string propertyName)
    {
        var values = new List<string>();
        config.EvaluatePropertyValues("/Script/JsonAsAsset.JsonAsAssetSettings", propertyName, values);

        return values.Count == 1 ? values[0] : "";
    }

    public List<string> GetArrayProperty(ConfigIni config, string propertyName)
    {
        var values = new List<string>();
        config.EvaluatePropertyValues("/Script/JsonAsAsset.JsonAsAssetSettings", propertyName, values);

        return values;
    }

    public bool GetBoolProperty(ConfigIni config, string propertyName, bool @default = false)
    {
        var values = new List<string>();
        config.EvaluatePropertyValues("/Script/JsonAsAsset.JsonAsAssetSettings", propertyName, values);

        if (values.Count == 0)
            return @default;

        return values[0] == "True";
    }

    public string GetPathProperty(ConfigIni config, string propertyName)
    {
        var values = new List<string>();
        config.EvaluatePropertyValues("/Script/JsonAsAsset.JsonAsAssetSettings", propertyName, values);

        return values.Count == 0 ? "" : values[0].SubstringBeforeLast("\"").SubstringAfterLast("\"");
    }

    // Find config folder & UpdateData
    public ConfigIni GetEditorConfig()
    {
        string configFolder = System.AppDomain.CurrentDomain.BaseDirectory.SubstringBeforeLast("\\Plugins\\") + "\\Config\\";
        ConfigIni config = new ConfigIni("DefaultEditorPerProjectUserSettings");
        config.Read(File.OpenText(configFolder + "DefaultEditorPerProjectUserSettings.ini"));

        // Set Config Data to class
        MappingFilePath = GetPathProperty(config, "MappingFilePath");
        ArchiveDirectory = GetPathProperty(config, "ArchiveDirectory");
        UnrealVersion = (EGame)Enum.Parse(typeof(EGame), GetStringProperty(config, "UnrealVersion"), true);
        ArchiveKey = GetStringProperty(config, "ArchiveKey");

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        if (MappingFilePath != "") WriteLog("UserSettings", ConsoleColor.Blue, $"Mappings: {MappingFilePath.SubstringBeforeLast("\\")}");
        WriteLog("UserSettings", ConsoleColor.Blue, $"Archive Directory: {ArchiveDirectory.SubstringBeforeLast("\\")}");
        WriteLog("UserSettings", ConsoleColor.Blue, $"Unreal Versioning: {UnrealVersion.ToString()}");

        return config;
    }

    public async void RunEditorDataTask()
    {
        
    }

    public override async Task Initialize()
    {
        WriteLog("CORE", ConsoleColor.Cyan, "Initializing FetchContext, and provider..");

        // Find config folder
        string configFolder = System.AppDomain.CurrentDomain.BaseDirectory.SubstringBeforeLast("\\Plugins\\") + "\\Config\\";
        WriteLog("UserSettings", ConsoleColor.Blue, $"Found config folder: {configFolder.SubstringBeforeLast("\\")}");
        
        // DefaultEditorPerProjectUserSettings
        ConfigIni config = GetEditorConfig();
        
        Provider = new DefaultFileProvider(ArchiveDirectory, SearchOption.TopDirectoryOnly, true, new VersionContainer(UnrealVersion));
        
        Provider.VfsRegistered += (sender, count) =>
        {
            if (sender is not IAesVfsReader reader) return;
            ApplicationStatus.UpdateLabel($"Registered {count} ({reader.Name})", "CUE4Parse");
        };
        Provider.VfsMounted += (sender, count) =>
        {
            if (sender is not IAesVfsReader reader) return;
            ApplicationStatus.UpdateLabel($"Mounted {count:N0} ({reader.Name})", "CUE4Parse");
        };
        Provider.VfsUnmounted += (sender, _) =>
        {
            if (sender is not IAesVfsReader reader) return;
        };

        // Create new file provider
        Provider.Initialize();
        
        InternalGameName = ArchiveDirectory.Replace("/", "\\").SubstringBeforeLast(ArchiveDirectory.Contains("eFootball") ? "\\pak" : "\\Content").SubstringAfterLast("\\");
        
        var oodlePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, OodleHelper.OODLE_DLL_NAME);
        if (!File.Exists(oodlePath))
        {
            await OodleHelper.DownloadOodleDllAsync(oodlePath);
        }
        OodleHelper.Initialize(oodlePath);

        if (!string.IsNullOrEmpty(ArchiveKey))
        {
            // Submit Main AES Key
            await Provider.SubmitKeyAsync(new FGuid(), new FAesKey(ArchiveKey));
            WriteLog("Provider", ConsoleColor.Red, $"Submitted Archive Key: {ArchiveKey}");
        }

        var dynamicKeys = GetArrayProperty(config, "DynamicKeys");
        if (dynamicKeys.Count != 0)
            WriteLog("Provider", ConsoleColor.Red, "Reading " + dynamicKeys.Count + " Dynamic Keys -------------------------------------------");

        // Submit each dynamic key
        foreach (string key in dynamicKeys)
        {
            var _key = key; var ReAssignedKey = _key.SubstringAfterLast("(").SubstringBeforeLast(")");
            string[] entries = ReAssignedKey.Split(",");

            // Key & Guid
            var Key = entries[0].SubstringBeforeLast("\"").SubstringAfterLast("\"");
            var Guid = entries[1].SubstringBeforeLast("\"").SubstringAfterLast("\"");

            await Provider.SubmitKeyAsync(new FGuid(Guid), new FAesKey(Key));
            WriteLog("Provider", ConsoleColor.Red, $"Submitted Key: {Key}");
        }

        if (MappingFilePath != "") Provider.MappingsContainer = new FileUsmapTypeMappingsProvider(MappingFilePath);
        Provider.LoadLocalization(ELanguage.English);
        Provider.LoadVirtualPaths();
        
        ApplicationStatus.SetStatus(EApplicationStatus.Completed);
        ApplicationStatus.UpdateLabel($"Initialized provider successfully", "CUE4Parse");
        
        ApplicationStatus.IsReady = true;
        
        LogToConsole($"[CUE4Parse] Initialized provider successfully");
    }
}
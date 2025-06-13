using System.IO;
using CUE4Parse.Compression;
using CUE4Parse.Encryption.Aes;
using CUE4Parse.FileProvider;
using CUE4Parse.MappingsProvider;
using CUE4Parse.UE4.Objects.Core.Misc;
using CUE4Parse.UE4.Versions;
using CUE4Parse.Utils;
using UE4Config.Parsing;

namespace LocalFetch.Utilities;

public class FetchContext
{
    public static DefaultFileProvider? Provider;
    private static string? MappingFilePath;
    private static string? ArchiveDirectory;
    private static EGame UnrealVersion;
    private static string? ArchiveKey;

    public static string GetStringProperty(ConfigIni config, string propertyName)
    {
        var values = new List<string>();
        config.EvaluatePropertyValues("/Script/JsonAsAsset.JsonAsAssetSettings", propertyName, values);

        return values.Count == 1 ? values[0] : "";
    }

    public static List<string> GetArrayProperty(ConfigIni config, string PropertyName)
    {
        var values = new List<string>();
        config.EvaluatePropertyValues("/Script/JsonAsAsset.JsonAsAssetSettings", PropertyName, values);

        return values;
    }

    public bool GetBoolProperty(ConfigIni config, string PropertyName, bool Default = false)
    {
        var values = new List<string>();
        config.EvaluatePropertyValues("/Script/JsonAsAsset.JsonAsAssetSettings", PropertyName, values);

        if (values.Count == 0)
            return Default;

        return values[0] == "True";
    }

    public static string GetPathProperty(ConfigIni config, string PropertyName)
    {
        var values = new List<string>();
        config.EvaluatePropertyValues("/Script/JsonAsAsset.JsonAsAssetSettings", PropertyName, values);

        return values.Count == 0 ? "" : values[0].SubstringBeforeLast("\"").SubstringAfterLast("\"");
    }

    // Find config folder & UpdateData
    public static ConfigIni GetEditorConfig()
    {
        var config_folder = AppDomain.CurrentDomain.BaseDirectory.SubstringBeforeLast(@"\Plugins\") + @"\Config\";
        var config = new ConfigIni("DefaultEditorPerProjectUserSettings");
        config.Read(File.OpenText(config_folder + "DefaultEditorPerProjectUserSettings.ini"));

        // Set Config Data to class
        MappingFilePath = GetPathProperty(config, "MappingFilePath");
        ArchiveDirectory = GetPathProperty(config, "ArchiveDirectory");
        UnrealVersion = (EGame)Enum.Parse(typeof(EGame), GetStringProperty(config, "UnrealVersion"), true);
        ArchiveKey = GetStringProperty(config, "ArchiveKey");

        if (MappingFilePath != "") Logger.Log($"Mappings File: {MappingFilePath.SubstringAfterLast("/")}", LogType.Configuration);
        Logger.Log($"Directory: {ArchiveDirectory.SubstringBeforeLast("\\")}", LogType.Configuration);
        Logger.Log($"Unreal Engine: {UnrealVersion.ToString()}", LogType.Configuration);

        return config;
    }

    public async Task Initialize()
    {
        Logger.Log("Initializing FetchContext, and provider..");

        // DefaultEditorPerProjectUserSettings
        var config = GetEditorConfig();

        // Create new file provider
        Provider = new DefaultFileProvider(ArchiveDirectory!, SearchOption.AllDirectories, new VersionContainer(UnrealVersion), StringComparer.OrdinalIgnoreCase);
        Provider.Initialize();

        var oodlePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, OodleHelper.OODLE_DLL_NAME);
        if (!File.Exists(oodlePath)) await OodleHelper.DownloadOodleDllAsync(oodlePath);
        OodleHelper.Initialize(oodlePath);

        if (!string.IsNullOrEmpty(ArchiveKey))
        {
            // Submit Main AES Key
            await Provider.SubmitKeyAsync(new FGuid(), new FAesKey(ArchiveKey));
            Logger.Log($"Submitted Key: {ArchiveKey}", LogType.CUE4);
        }

        var DynamicKeys = GetArrayProperty(config, "DynamicKeys");

        // Submit each dynamic key
        foreach (var key in DynamicKeys)
        {
            var ReAssignedKey = key.SubstringAfterLast("(").SubstringBeforeLast(")");
            var entries = ReAssignedKey.Split(",");

            // Key & Guid
            var Key = entries[0].SubstringBeforeLast("\"").SubstringAfterLast("\"");
            var Guid = entries[1].SubstringBeforeLast("\"").SubstringAfterLast("\"");

            await Provider.SubmitKeyAsync(new FGuid(Guid), new FAesKey(Key));
            Logger.Log($"Submitted Dynamic Key: {Key}", LogType.CUE4);
        }

        if (MappingFilePath != null) Provider.MappingsContainer = new FileUsmapTypeMappingsProvider(MappingFilePath);
        Provider.LoadVirtualPaths();
        Provider.Mount();
    }
}
using System;
using System.IO;
using CUE4Parse.UE4.Objects.Core.Misc;

namespace LocalFetch.Shared;

public static class Globals
{
    public const string VersionString = "1.0.0";

    public static readonly string ApplicationDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    public const string FolderName = "LocalFetch";
    
    public static readonly string DataFolder = Path.Combine(ApplicationDataFolder, FolderName);
    public static readonly DirectoryInfo BuildsFolder = new(Path.Combine(DataFolder, "Builds"));
    public static readonly DirectoryInfo CacheFolder = new(Path.Combine(DataFolder, "Cache"));
    
    public static readonly FGuid EMPTY_GUID = new();
    public const string EMPTY_CHAR = "0x0000000000000000000000000000000000000000000000000000000000000000";
    
    public static readonly DirectoryInfo ApplicationLocation = new(AppDomain.CurrentDomain.BaseDirectory);
}

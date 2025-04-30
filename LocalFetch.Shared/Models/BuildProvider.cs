using System;
using System.IO;

using CUE4Parse.UE4.Readers;
using CUE4Parse.UE4.Versions;
using CUE4Parse.FileProvider.Vfs;

namespace LocalFetch.Shared.Models;

public class BuildProvider : AbstractVfsFileProvider
{
    private readonly DirectoryInfo WorkingDirectory;
    private static readonly EnumerationOptions EnumerationOptions = new()
    {
        RecurseSubdirectories = true,
        IgnoreInaccessible = true
    };

    public BuildProvider(string directory, VersionContainer? version = null) : base(version, StringComparer.OrdinalIgnoreCase)
    {
        WorkingDirectory = new DirectoryInfo(directory);
        SkipReferencedTextures = true;
    }

    public override void Initialize()
    {
        if (!WorkingDirectory.Exists) throw new DirectoryNotFoundException($"Build installation folder doesn't exist: {WorkingDirectory.FullName}");
        RegisterFiles(WorkingDirectory);
    }

    public void RegisterFiles(DirectoryInfo directory)
    {
        foreach (var file in directory.EnumerateFiles("*.*", EnumerationOptions))
        {
            RegisterVfs(file.FullName, [ file.OpenRead() ], it => new FStreamArchive(it, File.OpenRead(it), Versions));
        }
    }
}
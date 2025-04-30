using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using CUE4Parse.Utils;
using CUE4Parse.UE4.Assets.Exports;
using CUE4Parse.UE4.Assets.Exports.Texture;
using CUE4Parse.UE4.Assets.Exports.Sound;
using CUE4Parse_Conversion.Textures;
using CUE4Parse_Conversion.Sounds;
using CUE4Parse.UE4.Objects.Meshes;
using Newtonsoft.Json;

using LocalFetch.Shared.Models;
using LocalFetch.Shared.Settings.Builds;
using LocalFetch.Shared.Convertors;

/* ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ */
/* Local Fetch API Controller */
/* ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ */

namespace LocalFetch.API.Controllers;

[Route("api")]
[ApiController]
public class LocalFetchApiController : ControllerBase
{
    /* ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ */
    public static IEnumerable<Build> builds = [];
    public static Build? mainBuild => builds.Any() ? builds.First() : null;
    public static bool IsBuildInitialized => builds.Any() && mainBuild is { IsInitialized: true };
    /* ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ */
    
    /* Metadata request to retrieve information about this process */
    [HttpGet("metadata")]
    public ActionResult Get()
    {
        if (!IsBuildInitialized) return new BadRequestObjectResult(JsonConvert.SerializeObject(new
        {
            reason = "Not initialized yet"
        }, Formatting.Indented));

        return new OkObjectResult(JsonConvert.SerializeObject(new
        {
            name = mainBuild?.Provider.ProjectName
        }, Formatting.Indented));
    }
    
    /* Normal Export */
    [HttpGet("export")]
    public ActionResult Get(bool raw, string? path)
    {
        if (!IsBuildInitialized) return BadRequest();
        if (path == null) return BadRequest();
        
        var contentType = Request.Headers.ContentType;
        path = path.SubstringBefore('.');
        
        /* Find the build that'll have this asset */
        var build = FindBuildForPath(path, found: out var found);
        if (!found) return new NotFoundResult();
        
        var provider = build.Provider;
        provider.TryLoadPackageObject(path, export: out var localObject);

        /* Return a raw export */
        if (raw) return HandleRawExport(path, provider);

        /* Switch on Class Type */
        return localObject switch
        {
            UTexture texture => ProcessTexture(texture, contentType!),
            USoundWave wave => ProcessSoundWave(wave),
            _ => HandleRawExport(path, provider)
        };
    }

    /* Return a texture as a file / encoding */
    private ActionResult ProcessTexture(UTexture texture, string contentType)
    {
        if (texture.GetFirstMip()?.BulkData.Data is { } mipData && contentType == "application/octet-stream")
        {
            return File(mipData, contentType);
        }

        var textureData = texture.Decode();
        if (textureData == null)
        {
            return StatusCode(500, new
            {
                errored = true,
                exceptionstring = "Invalid texture data, exported as json",
                jsonOutput = new { texture }
            });
        }

        return StatusCode(500);
    }

    /* Return a sound wave file format */
    private ActionResult ProcessSoundWave(USoundWave wave)
    {
        wave.Decode(true, out var audioFormat, out var data);
        
        if (data == null || string.IsNullOrEmpty(audioFormat))
        {
            return Conflict(new
            {
                errored = true,
                exceptionstring = "Invalid audio data, returned raw export as json",
                jsonOutput = new[] { wave }
            });
        }

        var mimeType = audioFormat.ToLower() switch
        {
            "wem" => "application/vnd.wwise.wem",
            "wav" => "audio/wav",
            "adpcm" => "audio/adpcm",
            "opus" => "audio/opus",
            _ => "audio/ogg"
        };

        return File(data, mimeType);
    }

    /* Handle raw exports */
    public ActionResult HandleRawExport(string path, BuildProvider provider)
    {
        var objectPath = $"{path.SubstringBefore('.')}.o.uasset";
            
        var pkg = provider.LoadPackage(path);
        var exports = pkg.GetExports().ToArray();
        var finalExports = new List<UObject>(exports);

        var mergedExports = new List<UObject>();
        if (provider.TryLoadPackage(objectPath, out var editorAsset))
        {
            foreach (var export in exports)
            {
                var editorData = editorAsset.GetExportOrNull($"{export.Name}EditorOnlyData");
                if (editorData == null)
                {
                    continue;
                }
                
                export.Properties.AddRange(editorData.Properties);
                mergedExports.Add(export);
            }

            finalExports.AddRange(editorAsset.GetExports().Where(editorExport => !mergedExports.Contains(editorExport)));
        }
        mergedExports.Clear();

        var converters = new Dictionary<Type, JsonConverter> {{ typeof(FColorVertexBuffer), new FColorVertexBufferCustomConverter() }};
        var settings = new JsonSerializerSettings { ContractResolver = new FColorVertexBufferCustomResolver(converters!) };

        /* Serialize object, and return it indented */
        return new OkObjectResult(JsonConvert.SerializeObject(new
        {
            jsonOutput = finalExports
        }, Formatting.Indented, settings));
    }
    
    /*
     * If the path exists on the main build, it'll check if other builds specifically override the main build, if so it'll pick that, else it'll give the one found initially
     * If the path doesn't exist on a main build, it'll cycle through each build to find one that has the asset existing
     */
    public Build FindBuildForPath(string rawPath, out bool found)
    {
        var path = rawPath.SubstringBefore('.');
        found = false;
        
        if (mainBuild!.Provider.TryLoadPackage(path, package: out _))
        {
            found = true;

            foreach (var build in builds)
            {
                if (!build.IsInitialized) continue;
                if (!build.Provider.TryLoadPackage(path, package: out var package)) continue;
                var assetType = package.GetExports().FirstOrDefault()?.ExportType;

                if (assetType != null && build.SecondaryAssetTypes.Contains(assetType, StringComparer.OrdinalIgnoreCase))
                {
                    return build;
                }
            }
        }
        else
        {
            foreach (var build in builds)
            {
                if (!build.IsInitialized) continue;
                if (!build.Provider.TryLoadPackage(path, package: out _)) continue;
            
                found = true;
                
                return build;
            }
        }

        return mainBuild;
    }
}

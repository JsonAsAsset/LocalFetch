using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using CUE4Parse.Utils;
using CUE4Parse.UE4.Assets.Exports;
using CUE4Parse.UE4.Assets.Exports.Texture;
using CUE4Parse.UE4.Assets.Exports.Sound;
using CUE4Parse_Conversion.Textures;
using CUE4Parse_Conversion.Sounds;
using Newtonsoft.Json;

using CUE4Parse.FileProvider.Vfs;
using SkiaSharp;

/* -------------------------------------------------------------------------------------------------------------------------------- */
/* Local Fetch API Controller */
/* _provider is from the Local Fetch Web Application, as we don't want to start two different CUE4Parses */
/* -------------------------------------------------------------------------------------------------------------------------------- */

namespace LocalFetch.API.Controllers;

[Route("api")]
[ApiController]
public class LocalFetchApiController(DbContext context) : ControllerBase
{
    /* ----------------------------------------------------------------------------------------------------------------------- */
    private readonly DbContext _context = context;
    private readonly AbstractVfsFileProvider? fileProvider = LocalFetchApi.Provider; /* Set from LocalFetch
    /* ----------------------------------------------------------------------------------------------------------------------- */
        
    /* Normal Export */
    [HttpGet("export")]
    public ActionResult Get(bool raw, string path)
    {
        var contentType = Request.Headers.ContentType;
        path = path.SubstringBefore('.');
        
        try
        {
            var localObject = fileProvider?.LoadObject(path);

            /* Return a raw export */
            if (raw) return HandleRawExport(path);

            /* Switch on Class Type */
            return localObject switch
            {
                UTexture texture => ProcessTexture(texture, contentType!),
                USoundWave wave => ProcessSoundWave(wave),
                _ => HandleRawExport(path)
            };
        }
        catch (Exception exception)
        {
            return new ConflictObjectResult(JsonConvert.SerializeObject(new
            {
                errored = true,
                note = exception.Message.StartsWith("One or more errors occurred. (There is no game file with the path ")
                    ? "Unable to find package"
                    : exception.Message
            }, Formatting.Indented));
        }
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

        return File(textureData.Encode(SKEncodedImageFormat.Png, quality: 100).AsStream(), contentType);
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
    private ActionResult HandleRawExport(string path)
    {
        if (fileProvider == null)
        {
            return StatusCode(500);
        }
        
        var objectPath = path.SubstringBefore('.') + ".o.uasset";
            
        var pkg = fileProvider.LoadPackage(path);
        var exports = pkg.GetExports().ToArray();
        var finalExports = new List<UObject>(exports);

        var mergedExports = new List<UObject>();
        if (fileProvider.TryLoadPackage(objectPath, out var editorAsset))
        {
            foreach (var export in exports)
            {
                var editorData = editorAsset.GetExportOrNull(export.Name + "EditorOnlyData");
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

        // Serialize object, and return it indented
        return new OkObjectResult(JsonConvert.SerializeObject(new
        {
            jsonOutput = finalExports
        }, Formatting.Indented));
    }
}

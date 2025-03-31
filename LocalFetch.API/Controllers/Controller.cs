using Microsoft.AspNetCore.Mvc;
using CUE4Parse.Utils;
using CUE4Parse.UE4.Assets.Exports;
using CUE4Parse.UE4.Assets.Exports.Texture;
using CUE4Parse.UE4.Assets.Exports.Sound;
using CUE4Parse_Conversion.Textures;
using CUE4Parse_Conversion.Sounds;
using Newtonsoft.Json;
using SkiaSharp;
using CUE4Parse.FileProvider;
using CUE4Parse.UE4.Objects.Meshes;
using LocalFetch.Utilities;
using Newtonsoft.Json.Serialization;

class FColorVertexBufferCustomConverter : JsonConverter<FColorVertexBuffer>
{
    public override void WriteJson(JsonWriter writer, FColorVertexBuffer? value, JsonSerializer serializer)
    {
        writer.WriteStartObject();

        writer.WritePropertyName("Data");
        writer.WriteStartArray();
        
        foreach (var c in value!.Data)
            writer.WriteValue(UnsafePrint.BytesToHex(c.A, c.R, c.G, c.B));
        
        writer.WriteEndArray();

        writer.WritePropertyName("Stride");
        writer.WriteValue(value.Stride);

        writer.WritePropertyName("NumVertices");
        writer.WriteValue(value.NumVertices);

        writer.WriteEndObject();
    }

    public override FColorVertexBuffer ReadJson(JsonReader reader, Type objectType, FColorVertexBuffer? existingValue, bool hasExistingValue,
        JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }
}

class LocalFetchResolver : DefaultContractResolver
{
    private Dictionary<Type, JsonConverter> _Converters { get; set; }

    public LocalFetchResolver(Dictionary<Type, JsonConverter> converters)
    {
        _Converters = converters;
    }

    protected override JsonObjectContract CreateObjectContract(Type objectType)
    {
        JsonObjectContract contract = base.CreateObjectContract(objectType);
        if (_Converters.TryGetValue(objectType, out JsonConverter converter))
        {
            contract.Converter = converter;
        }
        return contract;
    }
}

namespace LocalFetch.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocalFetchController : ControllerBase
    {
        private readonly DefaultFileProvider? fileProvider = FetchContext.Provider;

        [HttpGet("/api/export")]
        public ActionResult Get(bool raw, string? path)
        {
            if (fileProvider == null) return BadRequest();
            if (path == null) return BadRequest();
            
            var contentType = Request.Headers.ContentType;
            path = path.SubstringBefore('.');

            try
            {
                fileProvider.TryLoadPackageObject(path, export: out var localObject);

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
                return Conflict(new
                {
                    errored = true,
                    note = exception.Message.StartsWith("One or more errors occurred. (There is no game file with the path ")
                        ? "Unable to find package"
                        : exception.Message
                });
            }
        }

        [HttpGet("/api/name")]
        public ActionResult Get()
        {
            if (fileProvider == null) return BadRequest();
            
            return StatusCode(200, fileProvider.ProjectName);
        }

        private ActionResult ProcessTexture(UTexture texture, string contentType)
        {
            if (texture.GetFirstMip()!.BulkData.Data is { } mipData && contentType == "application/octet-stream")
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

        private ActionResult ProcessSoundWave(USoundWave wave)
        {
            wave.Decode(true, out var audioFormat, out var data);
            if (data == null || string.IsNullOrEmpty(audioFormat))
            {
                return Conflict(new
                {
                    errored = true,
                    exceptionstring = "Invalid audio data, exported as json",
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

        public ActionResult HandleRawExport(string? path)
        {
            if (path == null) return BadRequest();
            
            var objectPath = path.SubstringBefore('.') + ".o.uasset";
            
            var pkg = fileProvider!.LoadPackage(path);
            var exports = pkg.GetExports().ToArray();
            var finalExports = new List<UObject>(exports);

            var mergedExports = new List<UObject>();
            if (fileProvider.TryLoadPackage(objectPath, out var editorAsset))
            {
                foreach (var export in exports)
                {
                    var editorData = editorAsset.GetExportOrNull(export.Name + "EditorOnlyData");
                    if (editorData == null) continue;
                    
                    export.Properties.AddRange(editorData.Properties);
                    mergedExports.Add(export);
                }

                finalExports.AddRange(editorAsset.GetExports().Where(editorExport => !mergedExports.Contains(editorExport)));
            }
            mergedExports.Clear();
            
            var converters = new Dictionary<Type, JsonConverter>()
            {
                { typeof(FColorVertexBuffer), new FColorVertexBufferCustomConverter() }
            };
            var settings = new JsonSerializerSettings { ContractResolver = new LocalFetchResolver(converters) };

            // Serialize object, and return it indented
            return new OkObjectResult(JsonConvert.SerializeObject(new
            {
                jsonOutput = finalExports
            }, Formatting.Indented, settings));
        }
    }
}
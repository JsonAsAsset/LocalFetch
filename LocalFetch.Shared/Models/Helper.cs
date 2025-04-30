using System.IO;
using System.Threading.Tasks;
using CUE4Parse_Conversion.Textures;
using CUE4Parse_Conversion.Textures.BC;
using CUE4Parse.Compression;

namespace LocalFetch.Shared.Models;

public class Helper
{
    public static async Task LoadNativeLibraries(DirectoryInfo CacheFolder)
    {
        Directory.CreateDirectory(CacheFolder.ToString());

        var oodlePath = Path.Combine(CacheFolder.FullName, OodleHelper.OODLE_DLL_NAME);
        if (!File.Exists(oodlePath)) await OodleHelper.DownloadOodleDllAsync(oodlePath);
        OodleHelper.Initialize(oodlePath);
        
        var zlibPath = Path.Combine(CacheFolder.FullName, ZlibHelper.DLL_NAME);
        if (!File.Exists(zlibPath)) await ZlibHelper.DownloadDllAsync(zlibPath);
        ZlibHelper.Initialize(zlibPath);
        
        TextureDecoder.UseAssetRipperTextureDecoder = true;
        var detexPath = Path.Combine(CacheFolder.FullName, DetexHelper.DLL_NAME);
        if (!File.Exists(detexPath)) await DetexHelper.LoadDllAsync(detexPath);
        DetexHelper.Initialize(detexPath);
    }
}
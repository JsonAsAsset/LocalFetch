using CUE4Parse.FileProvider.Vfs;

namespace LocalFetch.API;

/* Startup of Local Fetch's API */
public class LocalFetchAPI
{
    public static AbstractVfsFileProvider? Provider;

    public LocalFetchAPI(AbstractVfsFileProvider provider)
    {
        Provider = provider;
    }

    public async Task RunApi(string[] args)
    {
    }
}
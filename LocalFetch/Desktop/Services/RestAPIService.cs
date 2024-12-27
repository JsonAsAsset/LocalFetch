using System.Threading.Tasks;
using LocalFetch.ViewModels;
using LocalFetchRestAPI;

namespace LocalFetch.Services
{
    // Starts the REST API for Local Fetch (used by JsonAsAsset)
    public sealed class RestAPIService
    {
        public CUE4ParseViewModel CUE4Parse { get; }

        public RestAPIService(CUE4ParseViewModel cue4parse)
        {
            CUE4Parse = cue4parse;
        }

        public async Task Initialize()
        {
            LocalFetchApi newLocalFetchApi = new LocalFetchApi(CUE4Parse.Provider);
            await newLocalFetchApi.RunApi([]);
        }
    }
}
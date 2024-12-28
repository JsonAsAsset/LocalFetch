using System;
using System.Threading.Tasks;
using LocalFetch.ViewModels;

// Starts the REST API for Local Fetch (used by JsonAsAsset)
//
// Uses LocalFetchRestAPI
// CUE4Parse Provider is sent to it on constructor

using LocalFetchRestAPI;

namespace LocalFetch.Services
{
    public sealed class RestApiService(CUE4ParseViewModel cue4Parse)
    {
        // ReSharper disable once InconsistentNaming
        private CUE4ParseViewModel CUE4Parse { get; } = cue4Parse;

        public async Task Initialize()
        {
            if (CUE4Parse.Provider != null)
            {
                var newLocalFetchApi = new LocalFetchApi(CUE4Parse.Provider);
                await newLocalFetchApi.RunApi([]);
            }
            else
            {
                throw new Exception("No provider found during initialization.");
            }
        }
    }
}
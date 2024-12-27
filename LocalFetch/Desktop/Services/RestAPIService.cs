﻿using LocalFetch.ViewModels;
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

        public void Initialize()
        {
            LocalFetchApi NewLocalFetchAPI = new LocalFetchApi(CUE4Parse.Provider);
            
            NewLocalFetchAPI.Start(new string[] { });
        }
    }
}
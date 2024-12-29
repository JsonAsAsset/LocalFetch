using System;
using System.Threading.Tasks;

using LocalFetch.API;
using LocalFetch.Shared;
using LocalFetch.Shared.Framework;
using Org.BouncyCastle.Pkix;
using Org.BouncyCastle.Pqc.Crypto.Falcon;

namespace LocalFetch.Services;

// Starts the REST API for Local Fetch (used by JsonAsAsset)
//
// Uses LocalFetch.API
// CUE4Parse Provider is sent to it on constructor

public sealed class RestApiService : ViewModelBase 
{
    public RestApiService()
    {
    }
    
    private bool _isapistarted;
    public bool IsAPIStarted
    {
        get => _isapistarted;
        set => SetProperty(ref _isapistarted, value);
    }
    
    private String _local_host_url;

    public RestApiService(string localHostUrl)
    {
        _local_host_url = localHostUrl;
    }

    public String LocalHostUrl
    {
        get => _local_host_url;
        set => SetProperty(ref _local_host_url, value);
    }
    
    public override async Task Initialize()
    {
        Console.WriteLine("RestApiService initialized");
        IsAPIStarted = true;

        if (CUE4ParseVM.Provider != null)
        {
            var newLocalFetchApi = new LocalFetchApi(CUE4ParseVM.Provider);
            newLocalFetchApi.RunApi([]);

            LocalHostUrl = Globals.LOCAL_FETCH_URL;
        }
        else
        {
            throw new Exception("No provider found during initialization.");
        }
    }
}
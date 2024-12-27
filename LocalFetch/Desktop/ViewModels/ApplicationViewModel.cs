using CUE4Parse.UE4.VirtualFileSystem;
using LocalFetch.Framework;
using LocalFetch.Services;

namespace LocalFetch.ViewModels;

public class ApplicationViewModel : ViewModel
{
    public string InitialWindowTitle => "Local Fetch";
    public string TitleExtra => "";
    public string GameDisplayName => "";
    
    public CUE4ParseViewModel CUE4Parse { get; }
    public RestAPIService RestApiService { get; }
    
    private FStatus _status;
    public FStatus Status
    {
        get => _status;
        private init => SetProperty(ref _status, value);
    }

    public ApplicationViewModel()
    {
        Status = new FStatus();

        CUE4Parse = new CUE4ParseViewModel();
        CUE4Parse.InitalizeProvider();

        if (CUE4Parse.Provider != null)
        {
            CUE4Parse.Provider.VfsRegistered += (sender, count) =>
            {
                if (sender is not IAesVfsReader reader) return;
                Status.UpdateStatusLabel($"Registered {count} ({reader.Name})", "CUE4Parse");
            };
            CUE4Parse.Provider.VfsMounted += (sender, count) =>
            {
                if (sender is not IAesVfsReader reader) return;
                Status.UpdateStatusLabel($"Mounted {count:N0} ({reader.Name})", "CUE4Parse");
            };
            CUE4Parse.Provider.VfsUnmounted += (sender, _) =>
            {
                if (sender is not IAesVfsReader reader) return;
            };
        }

        RestApiService = new RestAPIService(CUE4Parse);
    }
}
using CUE4Parse.UE4.VirtualFileSystem;
using LocalFetch.Framework;
using LocalFetch.Services;

namespace LocalFetch.ViewModels;

public class ApplicationViewModel : ViewModel
{
    public string WindowTitle => "Local Fetch [Development]";
    public string GameDisplayName => CUE4Parse.Provider.GameDisplayName ?? "Unknown";
    public string WindowTitleSubtext => "";
    
    public CUE4ParseViewModel CUE4Parse { get; }
    public RestApiService RestApiService { get; }
    
    private StatusIndicator _status;
    public StatusIndicator Status
    {
        get => _status;
        private init => SetProperty(ref _status, value);
    }

    public ApplicationViewModel()
    {
        Status = new StatusIndicator();

        CUE4Parse = new CUE4ParseViewModel();
        CUE4Parse.InitalizeProvider();

        if (CUE4Parse.Provider != null)
        {
            CUE4Parse.Provider.VfsRegistered += (sender, count) =>
            {
                if (sender is not IAesVfsReader reader) return;
                Status.UpdateLabel($"Registered {count} ({reader.Name})", "CUE4Parse");
            };
            CUE4Parse.Provider.VfsMounted += (sender, count) =>
            {
                if (sender is not IAesVfsReader reader) return;
                Status.UpdateLabel($"Mounted {count:N0} ({reader.Name})", "CUE4Parse");
            };
            CUE4Parse.Provider.VfsUnmounted += (sender, _) =>
            {
                if (sender is not IAesVfsReader reader) return;
            };
        }

        RestApiService = new RestApiService(CUE4Parse);
    }
}
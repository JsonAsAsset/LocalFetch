using LocalFetch.Framework;
using LocalFetch.Services;

namespace LocalFetch.ViewModels;

public class ApplicationViewModel : ViewModel
{
    public string InitialWindowTitle => "";
    public string TitleExtra => "";
    public string GameDisplayName => "";
    
    public CUE4ParseViewModel CUE4Parse { get; }
    public RestAPIService RestApiService { get; }

    public ApplicationViewModel()
    {
        CUE4Parse = new CUE4ParseViewModel();
        RestApiService = new RestAPIService(CUE4Parse);
    }
}
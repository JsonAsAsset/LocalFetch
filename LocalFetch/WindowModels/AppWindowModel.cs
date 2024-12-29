using System.Threading.Tasks;
using Avalonia.Interactivity;
using CUE4Parse.UE4.VirtualFileSystem;
using LocalFetch.Framework;
using LocalFetch.Services;
using LocalFetch.Shared.Framework;
using LocalFetch.Shared.Services;
using LocalFetch.ViewModels;

namespace LocalFetch.WindowModels;

public partial class AppWindowModel : WindowModelBase
{
    public StatusIndicator Indicator => ApplicationStatus;
    public RestApiService ApiServiceReference => RestApiVM;
}
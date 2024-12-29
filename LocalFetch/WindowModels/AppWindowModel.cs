using LocalFetch.Framework;
using LocalFetch.Services;

namespace LocalFetch.WindowModels;

public partial class AppWindowModel : WindowModelBase
{
    public StatusIndicator Indicator => ApplicationStatus;
    public RestApiService ApiServiceReference => RestApiVM;
}
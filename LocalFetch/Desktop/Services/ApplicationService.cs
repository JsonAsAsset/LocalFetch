using LocalFetch.ViewModels;

namespace LocalFetch.Services
{
    public sealed class ApplicationService
    {
        public static ApplicationViewModel ApplicationView { get; } = new();
    }
}
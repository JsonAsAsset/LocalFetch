using System.Threading.Tasks;
using System.Windows;
using AdonisUI.Controls;
using LocalFetch.Services;
using LocalFetch.ViewModels;

namespace LocalFetch
{
    public partial class MainWindow : AdonisWindow
    {
        private ApplicationViewModel _applicationView => ApplicationService.ApplicationView;

        public MainWindow()
        {
            DataContext = _applicationView;
            InitializeComponent();
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Load CUE4Parse and Start the API
            await _applicationView.CUE4Parse.Initialize();
            
            // Not async, causes issues
            _applicationView.RestApiService.Initialize();

            _applicationView.Status.SetStatus(EAppStatus.Completed);
            _applicationView.Status.UpdateStatusLabel($"Initialized provider successfully", "Local Fetch");

            _applicationView.Status.IsReady = true;
            
            Task.Run(async () =>
            {
                await Task.Delay(2000);
                _applicationView.Status.UpdateStatusLabel("Thank you for using our software!", "System");
            });
        }
    }
}
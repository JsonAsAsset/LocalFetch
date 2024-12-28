using System.Drawing;
using System.Threading.Tasks;
using System.Windows;
using AdonisUI;
using AdonisUI.Controls;
using LocalFetch.Services;
using LocalFetch.ViewModels;
using LocalFetch.Views.Resources.Controls;


namespace LocalFetch
{
    public partial class MainWindow : AdonisWindow
    {
        private ApplicationViewModel _applicationView => ApplicationService.ApplicationView;

        public MainWindow()
        {
            DataContext = _applicationView;
            InitializeComponent();
            
            AdonisUI.ResourceLocator.SetColorScheme(Application.Current.Resources, ResourceLocator.DarkColorScheme);
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Load CUE4Parse and Start the API
            await _applicationView.CUE4Parse.Initialize();
            
            // Not async, causes issues
            _applicationView.RestApiService.Initialize();

            _applicationView.Status.SetStatus(EApplicationStatus.Completed);
            _applicationView.Status.UpdateLabel($"Initialized provider successfully", "Local Fetch");

            _applicationView.Status.IsReady = true;
            
            Task.Run(async () =>
            {
                await Task.Delay(3500);
                _applicationView.Status.UpdateLabel("Thank you for using our software!", "System");
            });
        }
    }
}
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
            await _applicationView.RestApiService.Initialize();
        }
    }
}
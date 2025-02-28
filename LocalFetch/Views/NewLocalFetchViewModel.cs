using Avalonia.Controls;

namespace LocalFetch.Views
{
    public partial class NewLocalFetchView : Window
    {
        public NewLocalFetchView()
        {
            InitializeComponent();
        }

        private void CreateButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            // Logic for creating a new build
            Close(); // Close the window after creation
        }
    }
}
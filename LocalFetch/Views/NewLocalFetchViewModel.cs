using System;
using System.Collections.Generic;
using Avalonia.Controls;
using CUE4Parse.UE4.Versions;

namespace LocalFetch.Views
{
    public partial class NewLocalFetchView : Window
    {
        public List<EGame> Items { get; } = new List<EGame>((Enum.GetValues(typeof(EGame)) as EGame[])!);

        public NewLocalFetchView()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void CreateButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            // Logic for creating a new build
            Close(); // Close the window after creation
        }
    }
}
using System;
using System.Collections.Generic;
using Avalonia.Controls;
using CUE4Parse.UE4.Versions;

namespace LocalFetch.Views
{
    public partial class HomeWindow : Window
    {
        public HomeWindow()
        {
            InitializeComponent();
            DataContext = this;
        }
    }
}
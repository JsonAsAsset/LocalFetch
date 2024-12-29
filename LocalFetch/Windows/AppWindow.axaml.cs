using System;
using System.Diagnostics;
using Avalonia.Interactivity;
using LocalFetch.Shared.Framework;
using AppWindowModel = LocalFetch.WindowModels.AppWindowModel;

namespace LocalFetch.Windows;

public partial class AppWindow : WindowBase<AppWindowModel>
{
    public AppWindow() : base(initializeWindowModel: false)
    {
        InitializeComponent();
        DataContext = WindowModel;
    }
    
    public void onPressGithub(object sender, RoutedEventArgs args)
    {
        Process newProcess = new Process();

        try
        {
            newProcess.StartInfo.UseShellExecute = true;
            newProcess.StartInfo.FileName = "https://github.com/JsonAsAsset/LocalFetch";
            newProcess.Start();
        }
        catch (Exception e)
        {
        }
    }
}
using System;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using LocalFetch.Shared.Framework;
using LocalFetch.WindowModels;
using LocalFetch.Windows.Animation;

namespace LocalFetch.Windows;

public partial class AppWindow : WindowBase<AppWindowModel>
{
    private StatusBarAnimation _loadingStatusBarAnimation;

    public AppWindow() : base(initializeWindowModel: false)
    {
        InitializeComponent();
        DataContext = WindowModel;

        InitializeLoadingAnimation();
    }

    public void onPressGithub(object sender, RoutedEventArgs args)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/JsonAsAsset/LocalFetch",
                UseShellExecute = true
            });
        }
        catch (Exception e)
        {
            Debug.WriteLine($"Error opening URL: {e.Message}");
        }
    }
    
    // Animation --------------------------------------------------
    private void InitializeLoadingAnimation()
    {
        // Loading bar animation -------------------------------------------
        var loadingBorder = this.FindControl<Border>("LoadingBorder");
        _loadingStatusBarAnimation = new StatusBarAnimation(loadingBorder);
        
        DispatcherTimer.Run(() =>
        {
            if (WindowModel.Indicator.Status == EApplicationStatus.Loading)
            {
                _loadingStatusBarAnimation.StartLoadingAnimation();
            }
            else
            {
                _loadingStatusBarAnimation.StopLoadingAnimation(WindowModel.Indicator.Status);
            }

            return true;
        }, TimeSpan.FromMilliseconds(100));
    }
}
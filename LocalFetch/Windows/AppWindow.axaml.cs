using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using AvaloniaEdit;
using AvaloniaEdit.CodeCompletion;
using AvaloniaEdit.Highlighting;
using FluentAvalonia.Styling;
using LocalFetch.Extensions;
using LocalFetch.Shared.Framework;
using LocalFetch.WindowModels;
using LocalFetch.Windows.Animation;
using Microsoft.Win32;

namespace LocalFetch.Windows;

public partial class AppWindow : WindowBase<AppWindowModel>
{
    private StatusBarAnimation _loadingStatusBarAnimation;

    public AppWindow() : base(initializeWindowModel: false)
    {
        InitializeComponent();
        DataContext = WindowModel;

        InitializeLoadingAnimation();
        
        var faTheme = Avalonia.Application.Current?.Styles.OfType<FluentAvaloniaTheme>().FirstOrDefault();
        faTheme.CustomAccentColor = Color.Parse("#08335E");

        WindowModel.LogEditor = Editor2;
        Editor2.SyntaxHighlighting = new LogSyntaxHighlighter();
        
        Editor2.AppendText("[INFO] Setting up DataContext and loading models..\n\nI love executing! Said the program.");
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
    
    private const double MinFontSize = 8;
    private const double MaxFontSize = 48;
    private const double FontSizeStep = 1;
    
    private void Editor1_OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            e.Handled = true; // Mark the event as handled

            if (e.Delta.Y > 0)
            {
                Editor1.FontSize = Math.Min(Editor1.FontSize + FontSizeStep, MaxFontSize);
            }
            else if (e.Delta.Y < 0)
            {
                Editor1.FontSize = Math.Max(Editor1.FontSize - FontSizeStep, MinFontSize);
            }
        }
    }
}
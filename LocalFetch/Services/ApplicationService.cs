using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Documents;
using Avalonia.Input.Platform;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using DesktopNotifications;
using LocalFetch.Framework;
using LocalFetch.Shared.Framework;
using LocalFetch.Shared.Services;
using LocalFetch.ViewModels;
using Serilog;

using AppWindow = LocalFetch.Windows.AppWindow;
using AppWindowModel = LocalFetch.WindowModels.AppWindowModel;

namespace LocalFetch.Services;

public static class ApplicationService
{
    public static AppWindowModel AppWM => ViewModelRegistry.Get<AppWindowModel>()!;
    public static CUE4ParseViewModel CUE4ParseVM => ViewModelRegistry.Get<CUE4ParseViewModel>()!;
    public static StatusIndicator ApplicationStatus => ViewModelRegistry.Get<StatusIndicator>()!;
    public static RestApiService RestApiVM => ViewModelRegistry.Get<RestApiService>()!;
    public static IClassicDesktopStyleApplicationLifetime Application = null!;
    private static IStorageProvider StorageProvider => Application.MainWindow!.StorageProvider;
    public static IClipboard Clipboard => Application.MainWindow!.Clipboard!;
    public static INotificationManager? NotificationManager;

    public async static void Initialize()
    {
        ViewModelRegistry.New<StatusIndicator>();
        ViewModelRegistry.New<RestApiService>();
        ViewModelRegistry.New<CUE4ParseViewModel>();

        Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

        Application.MainWindow = new AppWindow();
        Application.Startup += OnStartup;
        Application.Exit += OnExit;
        
        Dispatcher.UIThread.UnhandledException += (sender, args) =>
        {
            args.Handled = true;
            HandleException(args.Exception);
        };
        
        TaskService.Exception += HandleException;
        
        Task.Run<Task>(async () =>
        {
            await CUE4ParseVM.Initialize();

            RestApiVM.Initialize();
        });
    }

    public static void LogToConsole(string message)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (!AppWM.LogEditor.Text.EndsWith(Environment.NewLine) && AppWM.LogEditor.Text.Length > 0)
            {
                AppWM.LogEditor.AppendText(Environment.NewLine);
            };
            AppWM.LogEditor.AppendText(message);
        });
    }
    
    public static void HandleException(Exception e)
    {
        var exceptionString = e.ToString();
        Log.Error(exceptionString);
    }
    
    public static void OnStartup(object? sender, ControlledApplicationLifetimeStartupEventArgs e)
    {
    }

    public static void OnExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
    {
        foreach (var viewModel in ViewModelRegistry.All())
        {
            viewModel.OnApplicationExit();
        }
    }
    
    public static void Launch(string location, bool shellExecute = true)
    {
        Process.Start(new ProcessStartInfo { FileName = location, UseShellExecute = shellExecute });
    }
    
    public static void LaunchSelected(string location)
    {
        var argument = "/select, \"" + location +"\"";
        Process.Start("explorer", argument);
    }
    
    public static async Task<string?> BrowseFolderDialog(string startLocation = "")
    {
        var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions { AllowMultiple = false, SuggestedStartLocation = await StorageProvider.TryGetFolderFromPathAsync(startLocation)});
        var folder = folders.ToArray().FirstOrDefault();

        return folder?.Path.AbsolutePath.Replace("%20", " ");
    }

    public static async Task<string?> BrowseFileDialog(string suggestedFileName = "", params FilePickerFileType[] fileTypes)
    {
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions { AllowMultiple = false, FileTypeFilter = fileTypes, SuggestedFileName = suggestedFileName});
        var file = files.ToArray().FirstOrDefault();

        return file?.Path.AbsolutePath.Replace("%20", " ");
    }

    public static async Task<string?> SaveFileDialog(string suggestedFileName = "", params FilePickerFileType[] fileTypes)
    {
        var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions {FileTypeChoices = fileTypes, SuggestedFileName = suggestedFileName});
        return file?.Path.AbsolutePath.Replace("%20", " ");
    }
}
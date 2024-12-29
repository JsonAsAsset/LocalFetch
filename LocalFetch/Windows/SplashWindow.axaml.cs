using System;
using System.Diagnostics;
using Avalonia.Interactivity;
using LocalFetch.Shared.Framework;
using AppWindowModel = LocalFetch.WindowModels.AppWindowModel;

namespace LocalFetch.Windows;

public partial class SplashWindow : WindowBase<AppWindowModel>
{
    public SplashWindow() : base(initializeWindowModel: false)
    {
    }
}
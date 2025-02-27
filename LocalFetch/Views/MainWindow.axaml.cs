using ActiproSoftware.Properties.Shared;
using Avalonia.Controls;

namespace LocalFetch.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        
        SR.SetCustomString(SRName.UICaptionButtonCloseText, null!);
        SR.SetCustomString(SRName.UICaptionButtonEnterFullScreenText, null!);
        SR.SetCustomString(SRName.UICaptionButtonExitFullScreenText, null!);
        SR.SetCustomString(SRName.UICaptionButtonMaximizeText, null!);
        SR.SetCustomString(SRName.UICaptionButtonMinimizeText, null!);
        SR.SetCustomString(SRName.UICaptionButtonRestoreText, null!);
    }
}
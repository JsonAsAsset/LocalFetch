using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace LocalFetch.Framework;

public class FStatus : ViewModel
{
    private Brush _statusTextColor;
    public Brush StatusTextColor
    {
        get => _statusTextColor;
        private set => SetProperty(ref _statusTextColor, value);
    }

    private bool bIsReady;
    public bool IsReady
    {
        get => bIsReady;
        set => SetProperty(ref bIsReady, value);
    }

    private EAppStatus AppStatus;
    public EAppStatus App
    {
        get => AppStatus;
        private set
        {
            SetProperty(ref AppStatus, value);
            IsReady = App != EAppStatus.Loading;
        }
    }

    private string _label;
    public string Label
    {
        get => _label;
        private set => SetProperty(ref _label, value);
    }

    public FStatus()
    {
        SetStatus(EAppStatus.Loading);
        _statusTextColor = Brushes.White;
    }

    private CancellationTokenSource _cancellationTokenSource;

    public void SetStatus(EAppStatus kind, string label = "")
    {
        StatusTextColor = Brushes.White;

        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource = new CancellationTokenSource();
    
        App = kind;
        UpdateStatusLabel(label);

        Task.Run(async () =>
        {
            try
            {
                await Task.Delay(2000, _cancellationTokenSource.Token);
            
                StatusTextColor = Brushes.Gray;
            }
            catch (OperationCanceledException)
            {
            }
        }, _cancellationTokenSource.Token);
    }

    public void UpdateStatusLabel(string label, string prefix = null)
    {
        Label = $"{$"[{prefix}]:" ?? App.ToString()} {label}".Trim();
    }
}
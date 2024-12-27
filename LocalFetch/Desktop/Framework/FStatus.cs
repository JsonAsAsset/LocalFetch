namespace LocalFetch.Framework;

public class FStatus : ViewModel
{
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
    }

    public void SetStatus(EAppStatus kind, string label = "")
    {
        App = kind;
        UpdateStatusLabel(label);
    }

    public void UpdateStatusLabel(string label, string prefix = null)
    {
        Label = $"{$"[{prefix}]:" ?? App.ToString()} {label}".Trim();
    }
}
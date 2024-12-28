namespace LocalFetch.Framework;

public class StatusIndicator : ViewModel
{
    // IsReady is true when loading is completed
    private bool _isReady;
    public bool IsReady
    {
        get => _isReady;
        set => SetProperty(ref _isReady, value);
    }

    // Enumeration to display the current status of the application
    private EApplicationStatus _status;
    public EApplicationStatus Status
    {
        get => _status;
        private set
        {
            SetProperty(ref _status, value);
            IsReady = Status != EApplicationStatus.Loading;
        }
    }

    // Label to display in the StatusBar
    private string _label;
    public string Label
    {
        get => _label;
        private set => SetProperty(ref _label, value);
    }
    
    // Update the label with a preview
    public void UpdateLabel(string label, string prefix = null)
    {
        Label = $"{$"[{prefix}]:" ?? Status.ToString()} {label}".Trim();
    }

    // Change the status and label
    public void SetStatus(EApplicationStatus newStatus, string label = "")
    {
        Status = newStatus;
        UpdateLabel(label);
    }
    
    public StatusIndicator()
    {
        SetStatus(EApplicationStatus.Loading);
    }
}
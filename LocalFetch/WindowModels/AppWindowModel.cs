using System;
using System.Threading.Tasks;
using System.Xml;
using Avalonia.Platform;
using AvaloniaEdit.Highlighting;
using AvaloniaEdit.Highlighting.Xshd;
using LocalFetch.Framework;
using LocalFetch.Services;

namespace LocalFetch.WindowModels;

public partial class AppWindowModel : WindowModelBase
{
    public StatusIndicator Indicator => ApplicationStatus;
    public RestApiService ApiServiceReference => RestApiVM;

    public string WindowTitle => $"Local Fetch";
    
    public static IHighlightingDefinition JsonHighlighter { get; set; }
    public AvaloniaEdit.TextEditor LogEditor { get; set; }
    
    static AppWindowModel()
    {
        using var stream = AssetLoader.Open(new Uri("avares://LocalFetch/Assets/Highlighters/Json.xshd"));
        using var reader = new XmlTextReader(stream);
        JsonHighlighter = HighlightingLoader.Load(reader, HighlightingManager.Instance);
    }
}
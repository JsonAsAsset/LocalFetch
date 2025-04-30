using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Avalonia;
using Avalonia.Media;

namespace LocalFetch.Shared.Settings.Builds.Containers;

/// <summary>
/// Visual colors displayed in menu settings
/// </summary>
public class BuildDisplayContainer
{
    public string GradientStartColor { get; set; }
    public string GradientEndColor { get; set; }
    
    public BuildDisplayContainer()
    {
        var colorCombos = new List<(string Start, string End)>
        {
            ("#3F71F0", "#7365F0"),
            ("#3F90BE", "#5A85E3"),
            ("#B18914", "#B98018"),
            ("#E18758", "#E7806B"),
            ("#9758E0", "#A54EE0"),
            ("#23998F", "#269CC7"),
            ("#58A162", "#409887")
        };

        var random = new Random();
        var selectedCombo = colorCombos[random.Next(colorCombos.Count)];
    
        GradientStartColor = selectedCombo.Start;
        GradientEndColor = selectedCombo.End;
    }
    
    [JsonIgnore]
    public LinearGradientBrush GradientBrush =>
        new()
        {
            StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
            EndPoint = new RelativePoint(1, 1, RelativeUnit.Relative),
            GradientStops =
            [
                new GradientStop(Color.Parse(GradientStartColor), 0),
                new GradientStop(Color.Parse(GradientEndColor), 1)
            ]
        };
}
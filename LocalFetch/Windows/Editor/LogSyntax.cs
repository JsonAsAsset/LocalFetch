using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using Avalonia.Media;
using AvaloniaEdit.Highlighting;

public class LogSyntaxHighlighter : IHighlightingDefinition
{
    public LogSyntaxHighlighter()
    {
        Name = "LogSyntax";
        MainRuleSet = CreateCustomRules();
    }

    public string Name { get; }
    public HighlightingRuleSet MainRuleSet { get; }
    public IEnumerable<HighlightingRuleSet> NamedRuleSets => new[] { MainRuleSet };
    public IDictionary<string, string> Properties => new Dictionary<string, string>();

    public IEnumerable<HighlightingColor?> NamedHighlightingColors
    {
        get
        {
            yield return null;
        }
    }

    public HighlightingRuleSet GetNamedRuleSet(string name) => null;

    public HighlightingColor? GetNamedColor(string name)
    {
        return null;
    }

    private HighlightingRuleSet CreateCustomRules()
    {
        var highlightingRules = new[]
        {
            // Rule for [WRN]
            new HighlightingRule
            {
                Regex = new Regex(@"\[WRN\]"),
                Color = new HighlightingColor
                {
                    Foreground = new SimpleHighlightingBrush(Color.Parse("#ffd35c")),
                    Name = "Warning"
                }
            },
            // Rule for [INFO]
            new HighlightingRule
            {
                Regex = new Regex(@"\[INFO\]"),
                Color = new HighlightingColor
                {
                    Foreground = new SimpleHighlightingBrush(Color.Parse("#5286ff")),
                    Name = "Information"
                }
            },
            // Rule for [CUE4Parse]
            new HighlightingRule
            {
                Regex = new Regex(@"\[CUE4Parse\]"),
                Color = new HighlightingColor
                {
                    Foreground = new SimpleHighlightingBrush(Color.Parse("#ffa352")),
                    Name = "CUE4Parse"
                }
            },
            // Rule for [API]
            new HighlightingRule
            {
                Regex = new Regex(@"\[RestAPI\]"),
                Color = new HighlightingColor
                {
                    Foreground = new SimpleHighlightingBrush(Color.Parse("#00d455")),
                    Name = "RestAPI"
                }
            },
            // Rule for Links
            new HighlightingRule
            {
                Regex = new Regex(@"https?:\/\/\S+"),
                Color = new HighlightingColor
                {
                    Foreground = new SimpleHighlightingBrush(Color.Parse("#0094ff")),
                    Name = "Links"
                }
            },
        };

        var ruleSet = new HighlightingRuleSet();
        foreach (var rule in highlightingRules)
        {
            ruleSet.Rules.Add(rule);
        }

        return ruleSet;
    }
}

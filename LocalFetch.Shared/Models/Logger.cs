using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace LocalFetch.Shared.Models;

public enum LogType
{
    [Description("C0RE")] Core,
    
    [Description("PARSE")] CUE4,

    [Description("INF0")] Info,

    [Description("ERROR")] Error,
    
    [Description("CREDITS")] Credits,
    
    [Description("CONFIG")] Configuration
}

public static class Logger
{
    private static readonly ConsoleColor[] CustomColors =
    [
        ConsoleColor.Green, ConsoleColor.Magenta, ConsoleColor.DarkYellow,
        ConsoleColor.DarkGreen, ConsoleColor.DarkMagenta, ConsoleColor.DarkBlue
    ];

    private static readonly Dictionary<string, ConsoleColor> CustomColorMap = new();
    private static readonly Random Rng = new();
    
    public static void Log(string message, LogType level = LogType.Core, string? CustomLevel = null)
    {
        var timestamp = DateTime.Now.ToString("H:m:s");
        var customLevelColor = ConsoleColor.White;

        if (CustomLevel != null)
        {
            if (!CustomColorMap.TryGetValue(CustomLevel, out customLevelColor))
            {
                customLevelColor = CustomColors[Rng.Next(CustomColors.Length)];
                CustomColorMap[CustomLevel] = customLevelColor;
            }
        }
        var levelColor = level switch
        {
            LogType.Core => ConsoleColor.Cyan,
            LogType.Info => ConsoleColor.Yellow,
            LogType.Error => ConsoleColor.Red,
            LogType.Credits => ConsoleColor.Blue,
            LogType.Configuration => ConsoleColor.Blue,
            LogType.CUE4 => ConsoleColor.DarkCyan,
            _ => throw new ArgumentOutOfRangeException(nameof(level), level, null)
        };

        Console.ForegroundColor = ConsoleColor.White;
        Console.Write($"[{timestamp}] [");
        if (!string.IsNullOrEmpty(CustomLevel))
        {
            Console.ForegroundColor = customLevelColor;
            Console.Write($"{CustomLevel}+");
        }
        Console.ForegroundColor = levelColor;
        Console.Write(level.GetLogTypeDescription());
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($"] {message}");
    }

    public static string? GetLogTypeDescription(this Enum value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var fieldInfo = value.GetType().GetField(value.ToString());
        
        return fieldInfo?.GetCustomAttribute<DescriptionAttribute>()?.Description;
    }
}
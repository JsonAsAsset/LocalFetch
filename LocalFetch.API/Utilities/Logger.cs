using System.ComponentModel;
using System.Reflection;

namespace LocalFetch.Utilities;

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
    public static void Log(string message, LogType level = LogType.Core)
    {
        Console.ForegroundColor = ConsoleColor.White;
        var timestamp = DateTime.Now.ToString("H:m:s");

        var levelColor = level switch
        {
            LogType.Core => ConsoleColor.Cyan,
            LogType.Info => ConsoleColor.Yellow,
            LogType.Error => ConsoleColor.Red,
            LogType.Credits => ConsoleColor.Blue,
            LogType.Configuration => ConsoleColor.Blue,
            LogType.CUE4 => ConsoleColor.Green,
            _ => throw new ArgumentOutOfRangeException(nameof(level), level, null)
        };

        Console.Write($"[{timestamp}] [");

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
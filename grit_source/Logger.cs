using System;
using Grit.Simulation.Helpers;

namespace Grit;

public static class Logger
{
    public enum LogType : byte
    {
        INFO = 0,
        WARN = 1,
        ERROR = 2
    }
    
    private static string Timestamp => DateTime.Now.ToString("dd/MM HH:mm:ss\t");

    private static string Tag(this LogType type) => type.ToString();

    private static ConsoleColor GetConsoleColor(this LogType type)
    {
        return type switch
        {
            LogType.INFO => ConsoleColor.Blue,
            LogType.WARN => ConsoleColor.Yellow,
            LogType.ERROR => ConsoleColor.Red,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    public static void Write(LogType type, object caller, string text)
    {
        if((byte)type < Settings.MIN_LOG_LEVEL) return;
        
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.Write($"{Timestamp}[{caller}.{type.Tag()}]:");
        Console.WriteLine();
        Console.SetCursorPosition(3, Console.CursorTop);
        Console.ForegroundColor = type.GetConsoleColor();
        Console.WriteLine(text);
        Console.WriteLine();

        Console.ResetColor();
    }
    
    public static void TestOk(string text)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        
        Console.Write($"{Timestamp}[OK]:");

        Console.ForegroundColor = ConsoleColor.Cyan;
        
        Console.SetCursorPosition(32, Console.CursorTop);
        Console.WriteLine(text);

        Console.ResetColor();
    }
    
    public static void TestError(string text)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        
        Console.Write($"{Timestamp}[ERROR]:");

        Console.ForegroundColor = ConsoleColor.Cyan;
        
        Console.SetCursorPosition(32, Console.CursorTop);
        Console.WriteLine(text);

        Console.ResetColor();
    }
    
    public static void Wait()
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        
        Console.Write('.');

        Console.ResetColor();
    }
}
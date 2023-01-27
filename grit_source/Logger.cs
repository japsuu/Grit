﻿using System;

namespace Grit;

public static class Logger
{
    public enum LogType
    {
        INFO,
        WARN,
        ERROR,
        SUCCESS
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
            LogType.SUCCESS => ConsoleColor.Green,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    public static void Write(LogType type, string text)
    {
        Console.ForegroundColor = type.GetConsoleColor();
        
        Console.Write($"{Timestamp}[{type.Tag()}]:");
        Console.SetCursorPosition(32, Console.CursorTop);
        Console.WriteLine(text);

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
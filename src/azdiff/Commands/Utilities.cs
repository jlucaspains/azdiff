using System.Text.RegularExpressions;

namespace azdiff;

static partial class Utilities
{
    public static void WriteInformation(string message, params object[] values)
    {
        WriteOut(message, ConsoleColor.White, ConsoleColor.Blue, values);
    }

    public static void WriteError(string message, params object[] values)
    {
        WriteOut(message, ConsoleColor.Red, ConsoleColor.Red, values);
    }

    public static void WriteSuccess(string message, params object[] values)
    {
        WriteOut(message, ConsoleColor.Green, ConsoleColor.Blue, values);
    }

    static void WriteOut(string message, ConsoleColor regularColor, ConsoleColor parameterColor, params object[] values)
    {
        var splits = TemplateRegex().Split(message);
        for (var i = 0; i < splits.Length; i++)
        {
            Console.ForegroundColor = regularColor;

            Console.Write(splits[i]);

            Console.ForegroundColor = parameterColor;

            if (i < values.Length)
                Console.Write(values[i]);
        }
        Console.WriteLine();
    }

    [GeneratedRegex("\\{[0-9]\\}")]
    private static partial Regex TemplateRegex();
}
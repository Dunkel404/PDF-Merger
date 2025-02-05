using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace PDFMerger;
public enum InputType
{
    StringInput,
    SwitchInput
}
public enum OutputType
{
    Success,
    Warning,
    Error,
    Information,
}
internal class Display
{
    public InputType InputType { get; set; }
    public OutputType OutputType { get; set; }

    private static void SuccessMessage (string message)
    {
        DisplayColoredMessage (message, ConsoleColor.Green);
    }
    private static void ErrorMessage (string message)
    {
        DisplayColoredMessage (message, ConsoleColor.Red);
    }
    private static void WarningMessage (string message)
    {
        DisplayColoredMessage (message, ConsoleColor.Yellow);
    }
    private static void InfoMessage (string message)
    {
        Console.WriteLine (message);
    }

    public static void OutputMessage (string message, OutputType outputType = OutputType.Information)
    {
        switch (outputType)
        {
            case OutputType.Warning:
                WarningMessage (message);
                break;
            case OutputType.Error:
                ErrorMessage (message);
                break;
            case OutputType.Information:
                InfoMessage (message);
                break;
            case OutputType.Success:
                SuccessMessage (message);
                break;
        }
    }

    private static bool SwitchInput (string message, bool defaultChoice)
    {
        string defaultLetter = defaultChoice ? "Y" : "N";
        Console.Write (message + $"[Y / N] (Default: {defaultLetter}): ");
        switch (Console.ReadLine ().ToLower ())
        {
            case "n" or "no":
                return false;
            case "y" or "yes":
                return true;
            default:
                return defaultChoice ? true : false;
        }
    }
    private static string StringInput (string message)
    {
        Console.Write (message);
        return Console.ReadLine ();
    }

    /// <summary>
    /// This method use Switch(bool) Input.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="defaultChoice"></param>
    /// <example>You like apples? </example>
    public static bool InputMessage (string message, bool defaultChoice)
    {
        return SwitchInput (message, defaultChoice);
    }
    /// <summary>
    /// This method use String Input.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="defaultChoice"></param>
    /// <example>File Input:...</example>
    public static string InputMessage (string message)
    {
        return StringInput (message);
    }
    private static void DisplayColoredMessage (string message, ConsoleColor consoleColor)
    {
        ChangeForeGroundColor (consoleColor);
        Console.WriteLine (message);
        ChangeForeGroundColor (ConsoleColor.White);
    }

    private static void ChangeForeGroundColor (ConsoleColor consoleColor)
    {
        Console.ForegroundColor = consoleColor;
    }
}

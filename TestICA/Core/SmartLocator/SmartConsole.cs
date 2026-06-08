using System;
using System.Runtime.InteropServices;
using TestICA.Core.Shell;

namespace TestICA.Core.SmartLocator;

public static class SmartConsole
{
    // Importation des fonctions natives de Windows pour forcer l'ouverture d'une console
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool AllocConsole();

    private static bool _isInitialized = false;

    public static void Initialize()
    {
        if (_isInitialized) return;

        // Ouvre la console Windows
        AllocConsole();
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.Title = "🔀 SmartLocator - Live Logcat Terminal";
        
        // Un petit écran d'accueil stylé
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("=======================================================================");
        Console.WriteLine("        SMARTLOCATOR LIVE TERMINAL (LOGCAT MODE) READY                 ");
        Console.WriteLine("=======================================================================");
        Console.ResetColor();
        
        _isInitialized = true;
    }

    public static void Log(string level, string tag, string message)
    {
        Initialize(); // S'assure que la console est ouverte

        string timeStamp = DateTime.Now.ToString("MM-dd HH:mm:ss.fff");
        
        // Gestion des couleurs selon le niveau (comme Logcat Android)
        switch (level.ToUpper())
        {
            case "D": // DEBUG
                Console.ForegroundColor = ConsoleColor.Blue;
                break;
            case "I": // INFO
                Console.ForegroundColor = ConsoleColor.Green;
                break;
            case "W": // WARN
                Console.ForegroundColor = ConsoleColor.Yellow;
                break;
            case "E": // ERROR
                Console.ForegroundColor = ConsoleColor.Red;
                break;
            default:
                Console.ResetColor();
                break;
        }

        // Structure du log : [Date Heure] [Niveau] [Tag] : Message
        Console.Write($"{timeStamp} {level.ToUpper()}/{tag}: ");
        Console.ResetColor();
        Console.WriteLine(message);
        SmartShell.WriteLog(level, tag, message);
    }
}
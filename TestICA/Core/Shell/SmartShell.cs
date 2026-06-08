using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;

namespace TestICA.Core.Shell;

public static class SmartShell
{
    private static Process _receiverProcess;
    private static TcpClient _client;
    private static StreamWriter _writer;
    private const int Port = 8888;
    private const string ScriptPath = @"C:\Users\basti\OneDrive\Desktop\LiveLog.ps1";

    /// <summary>
    /// Lance automatiquement le récepteur de logs et connecte le client TCP
    /// </summary>
    public static void Launch()
    {
        try
        {
            // 1. Lancer le script de réception en arrière-plan via PowerShell (mode Bypass automatique)
            if (File.Exists(ScriptPath) && (_receiverProcess == null || _receiverProcess.HasExited))
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoExit -ExecutionPolicy Bypass -File \"{ScriptPath}\"",
                    UseShellExecute = true, // Force l'ouverture d'une vraie fenêtre visible sur ton bureau
                    WindowStyle = ProcessWindowStyle.Normal
                };

                _receiverProcess = Process.Start(startInfo);
                
                // On laisse une seconde au serveur TCP PowerShell pour démarrer et se mettre à l'écoute
                System.Threading.Thread.Sleep(1000);
            }

            // 2. Connexion du canal de stream C# vers la fenêtre qui vient de s'ouvrir
            _client = new TcpClient("127.0.0.1", Port);
            _writer = new StreamWriter(_client.GetStream()) { AutoFlush = true };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SmartShell] Échec de l'auto-allumage du terminal : {ex.Message}");
        }
    }

    public static void WriteLog(string level, string tag, string message)
    {
        if (_writer == null) return;

        try
        {
            _writer.WriteLine($"{level}|{tag}|{message}");
        }
        catch
        {
            // Sécurité si le flux coupe
        }
    }

    /// <summary>
    /// Coupe proprement les connexions et ferme la fenêtre à la fin du test
    /// </summary>
    public static void Close()
    {
        try
        {
            _writer?.Close();
            _client?.Close();

            if (_receiverProcess != null && !_receiverProcess.HasExited)
            {
                _receiverProcess.Kill(); // Ferme la fenêtre PowerShell automatiquement à la fin
                _receiverProcess.Dispose();
            }
        }
        catch { }
    }
}
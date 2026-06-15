// Fichier : Tests/BaobaTesterBox.Tests/Infrastructure/LoggerTests.cs
using NUnit.Framework;
using BaobaTesterBox.Core.Loggings; // Utilise le bon namespace ici aussi

namespace BaobaTesterBox.Tests.Infrastructure;

[TestFixture]
public class LoggerTests
{
    [Test]
    public void Should_Write_Logs_In_All_Colors()
    {
        AppiumLoggerService.LogDebug("Ceci est un log de Debug (Gris)");
        AppiumLoggerService.LogInfo("Ceci est un log d'Info (Cyan) - Le système démarre !");
        AppiumLoggerService.LogWarn("Ceci est un avertissement (Jaune)...");
        AppiumLoggerService.LogError("Ceci est une simulation d'erreur (Rouge)", "LoggerTests", new System.Exception("Oups !"));
        
        Assert.Pass("Les logs ont été envoyés, regarde la console de ton exécuteur de test !");
    }
}
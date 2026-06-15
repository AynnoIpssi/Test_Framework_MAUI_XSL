// Fichier : src/BaobaTesterBox.Domain/Models/AppiumWaiterOptions.cs
namespace BaobaTesterBox.Domain.Models;

/// <summary>
/// Configuration de l'AppiumWaiterService : durées d'attente et fréquence de polling.
/// </summary>
public class AppiumWaiterOptions
{
    /// <summary>
    /// Durée maximale d'attente avant timeout, en secondes.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Intervalle entre deux vérifications, en millisecondes.
    /// </summary>
    public int PollingIntervalMs { get; set; } = 500;
}
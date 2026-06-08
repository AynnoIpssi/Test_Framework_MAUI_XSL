using System;

namespace TestICA.Core.Config.Models;

/// <summary>
/// Représente un écran spécifique de ton application mobile.
/// </summary>
public class AppScreen
{
    public string Name { get; set; } = string.Empty;
    public string UniqueElementId { get; set; } = string.Empty;

    public AppScreen(string name, string uniqueElementId)
    {
        Name = name;
        UniqueElementId = uniqueElementId;
    }
}

/// <summary>
/// Représente une transition (une route) reliant deux écrans entre eux.
/// </summary>
public class ScreenTransition
{
    public string FromScreen { get; set; } = string.Empty;
    public string ToScreen { get; set; } = string.Empty;
    public Action Action { get; set; }

    public ScreenTransition(string fromScreen, string toScreen, Action action)
    {
        FromScreen = fromScreen;
        ToScreen = toScreen;
        Action = action ?? throw new ArgumentNullException(nameof(action));
    }
}
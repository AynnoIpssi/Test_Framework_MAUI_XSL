using System;
using System.Collections.Generic;
using System.Linq;
using TestICA.Core.Config.Models; // <-- Indispensable pour lier tes modèles !

namespace TestICA.Core.Navigation;

public static class ApiNavigator
{
    private static readonly List<AppScreen> _screens = new();
    private static readonly List<ScreenTransition> _transitions = new();

    public static Func<string, bool>? IsElementVisibleCheck { get; set; }

    public static void RegisterScreen(string name, string uniqueElementId)
    {
        if (!_screens.Any(s => s.Name == name))
        {
            _screens.Add(new AppScreen(name, uniqueElementId));
        }
    }

    public static void RegisterTransition(string fromScreen, string toScreen, Action action)
    {
        _transitions.Add(new ScreenTransition(fromScreen, toScreen, action));
    }

    public static string DetectCurrentScreen()
    {
        if (IsElementVisibleCheck == null)
            throw new InvalidOperationException("[NAVIGATOR] Le vérificateur de visibilité n'est pas configuré.");

        foreach (var screen in _screens)
        {
            if (IsElementVisibleCheck.Invoke(screen.UniqueElementId))
            {
                return screen.Name;
            }
        }

        return "Unknown";
    }

    public static void GoTo(string targetScreenName)
    {
        string currentScreen = DetectCurrentScreen();

        if (currentScreen == "Unknown")
            throw new Exception("[NAVIGATOR] Impossible de naviguer : Écran actuel non identifié.");

        if (currentScreen == targetScreenName)
            return; 

        var path = CalculateShortestPath(currentScreen, targetScreenName);

        if (path == null || path.Count == 0)
            throw new Exception($"[NAVIGATOR] Aucun chemin trouvé entre '{currentScreen}' et '{targetScreenName}'.");

        foreach (var transition in path)
        {
            transition.Action.Invoke(); 
        }
    }

    private static List<ScreenTransition>? CalculateShortestPath(string start, string end)
    {
        var queue = new Queue<string>();
        var visited = new HashSet<string>();
        var cameFrom = new Dictionary<string, ScreenTransition>();

        queue.Enqueue(start);
        visited.Add(start);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            if (current == end)
            {
                var path = new List<ScreenTransition>();
                var step = end;
                while (step != start)
                {
                    var transition = cameFrom[step];
                    path.Add(transition);
                    step = transition.FromScreen;
                }
                path.Reverse();
                return path;
            }

            var availableTransitions = _transitions.Where(t => t.FromScreen == current);

            foreach (var transition in availableTransitions)
            {
                if (!visited.Contains(transition.ToScreen))
                {
                    visited.Add(transition.ToScreen);
                    cameFrom[transition.ToScreen] = transition;
                    queue.Enqueue(transition.ToScreen);
                }
            }
        }

        return null;
    }
}
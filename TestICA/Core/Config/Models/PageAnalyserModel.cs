using System;
using System.Collections.Generic;

namespace TestICA.Core.Config.Models;

public class PageAnalyserModel
{
    public string Url { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;

    // Cartographie des éléments de formulaire (id, valeur)
    public Dictionary<string, string> FormFields { get; set; } = new();

    // Liste des actions/boutons détectés sur la page
    public List<InteractiveElement> InteractiveElements { get; set; } = new();
    
    // ⚡ AJOUT RENFORCÉ : Devient une liste d'objets complexes pour gérer la visibilité des paragraphes
    public List<ParagraphElement> TextParagraphs { get; set; } = new();
}

public class InteractiveElement
{
    public string Id { get; set; } = string.Empty;
    public string TagName { get; set; } = string.Empty; 
    public string Text { get; set; } = string.Empty;
    public bool IsVisible { get; set; }
    public bool IsEnabled { get; set; }
    public string OnClickAttribute { get; set; } = string.Empty;
}

// ⚡ NOUVEAU COMPOSANT
public class ParagraphElement
{
    public string Text { get; set; } = string.Empty;
    public bool IsVisible { get; set; }
}
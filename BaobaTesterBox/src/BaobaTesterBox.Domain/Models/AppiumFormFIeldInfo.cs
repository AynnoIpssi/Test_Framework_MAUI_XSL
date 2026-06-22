using System.Collections.Generic;

namespace BaobaTesterBox.Domain.Models;

public class AppiumFormScanResult
{
    public string Mode { get; set; } = "unknown"; // "creation" ou "existing"
    public List<AppiumFormFieldInfo> Fields { get; set; } = new();

    public bool EstModeCreation => Mode == "creation";
}

public class AppiumFormFieldInfo
{
    public string Tag { get; set; } = "";
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public string Value { get; set; } = "";
    public bool? Checked { get; set; }
    public string SelectedText { get; set; } = "";
    public List<AppiumFormFieldOption> Options { get; set; } = new();
    public string Label { get; set; } = "";
    public bool Obligatory { get; set; }
    public string Placeholder { get; set; } = "";
}

public class AppiumFormFieldOption
{
    public string Value { get; set; } = "";
    public string Text { get; set; } = "";
}
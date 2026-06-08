namespace TestICA.Core.Config.Models;

public class SmartLocatorModel
{
    public int TimeoutInSecond { get; set; }
    
    public string RootContainerTagName { get; set; }
    
    public bool EnableVerboseLogging { get; set; }
}
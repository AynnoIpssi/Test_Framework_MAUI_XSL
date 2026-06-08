namespace TestICA.Core.Config.Models;

public class LogModel
{
    public string LogDirectory { get; set; }
    public string MinimumLevel { get; set; }
    
    public enum Type
    {
        Debug,
        Info,
        Warn,
        Error
    }
    
    public Type LogType { get; set; }
    
    public DateTime  LogDateTime { get; set; }
    
    public string? LogOrigine { get; set; }
    
    public string LogContent { get; set; }
    
    public Exception? Exception { get; set; }
}
    using Microsoft.Extensions.Configuration;
    using TestICA.Core.Config.Models;


    namespace TestICA.Core.Config;

    public class ConfigReader
    {
        public static AppiumModel Appium { get; private set; }
        public static LogModel Logger { get; private set; }
        public static SmartLocatorModel SmartLocator { get; private set; }

        static ConfigReader()
        {
            
            var builder = new ConfigurationBuilder();
            
            builder.SetBasePath(AppDomain.CurrentDomain.BaseDirectory);
            builder.AddJsonFile("Data/appsettings.json", optional: false, reloadOnChange: true);
            
            var root = builder.Build();
            
            //<---------------(AppiumStarter :)--------------->
            Appium = root.GetSection("AppiumConfig").Get<AppiumModel>()!;
            
            //<------------------(Loggers :)------------------>
            Logger = root.GetSection("LoggerConfig").Get<LogModel>();
            
            //<------------------(Loggers :)------------------>
            SmartLocator = root.GetSection("SmartLocatorConfig").Get<SmartLocatorModel>();
            
        }
    }   
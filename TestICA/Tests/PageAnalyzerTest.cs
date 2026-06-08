using System;
using System.IO;
using NUnit.Framework; 
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using TestICA.Core.ScriptExecutor;
using TestICA.Core.Config.Models;
using Enum = TestICA.Core.ScriptExecutor.Enum;

namespace TestICA.Tests;

[TestFixture] 
public class PageAnalyserTest
{
    private IWebDriver? _driver;
    private JsExecutor? _executor;

    [SetUp] 
    public void Setup()
    {
        var options = new ChromeOptions();
        options.AddArgument("--headless"); 
        
        _driver = new ChromeDriver(options);

        _executor = new JsExecutor
        {
            configDriver = _driver,
            // Ton chemin mis à jour qui pointe bien vers le dossier Core
            pathScript = Path.Combine(AppContext.BaseDirectory, "../../../Core/ScriptJS")
        };
    }

    [Test] 
    public void Test_PageAnalyser_ShouldReturnValidModel()
    {
        // 1. Arrange : On met le slash à la fin pour correspondre à ce que le navigateur va charger
        string testUrl = "https://example.com/";
        _driver!.Navigate().GoToUrl(testUrl);

        // 2. Act : On lance le scan
        var result = _executor!.Execute(Enum.JsScriptType.PageAnalyser);

        // 3. Assert : Vérifications NUnit
        Assert.That(result, Is.Not.Null, "Le résultat de l'exécution ne doit pas être null.");
        Assert.That(result, Is.InstanceOf<PageAnalyserModel>(), "Le résultat doit être converti en PageAnalyserModel.");

        var model = (PageAnalyserModel)result!;

        // Sécurité : On retire les slashes de fin sur les deux URLs avant de comparer
        Assert.That(model.Url.TrimEnd('/'), Is.EqualTo(testUrl.TrimEnd('/')), "L'URL capturée ne correspond pas.");
        Assert.That(string.IsNullOrEmpty(model.Title), Is.False, "Le titre de la page ne devrait pas être vide.");

        Console.WriteLine($"[TEST SUCCESS] Page analysée avec succès : {model.Title}");
    }

    [TearDown] 
    public void Teardown()
    {
        if (_driver != null)
        {
            _driver.Quit();
            _driver.Dispose();
        }
    }
}
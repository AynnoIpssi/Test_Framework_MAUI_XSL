using System;
using BaobaTesterBox.Core.ScriptExecutor;
using BaobaTesterBox.Core.Loggings;
using BaobaTesterBox.Domain.Models;
using BaobaTesterBox.Core.ScriptExecutor.Enum;

namespace BaobaTesterBox.Core.Navigation;

public class AppiumNavigationService
{
    private readonly AppiumJsExecutor _jsExecutor;
    private const string Origine = "AppiumNavigationService";

    public AppiumNavigationService()
    {
        _jsExecutor = new AppiumJsExecutor();
    }

    public void Go(int rubriqueId, int actionId = 1, string p1 = "", string p2 = "", string p3 = "", string p4 = "")
    {
        AppiumLoggerService.LogInfo($"[Navigation] Go → rubrique {rubriqueId}", Origine);
        _jsExecutor.Execute(JsScriptType.AppiumNavigationGo,
            actionId, rubriqueId, p1, p2, p3, p4);
    }

    public void ModuleGo(int rubriqueId, int moduleId, int batchId)
    {
        AppiumLoggerService.LogInfo($"[Navigation] ModuleGo → rubrique {rubriqueId}", Origine);
        _jsExecutor.Execute(JsScriptType.AppiumNavigationModuleGo,
            moduleId, rubriqueId, batchId);
    }

    public void Naviguer(AppiumNavigationOptions options)
    {
        if (options.ModuleId.HasValue && options.BatchId.HasValue)
            ModuleGo(options.RubriqueId, options.ModuleId.Value, options.BatchId.Value);
        else
            Go(options.RubriqueId, options.ActionId, options.Param1, options.Param2, options.Param3, options.Param4);
    }
}
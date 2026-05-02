using Autodesk.Revit.UI;
using BimAiAssistant.Commands;
using BimAiAssistant.Utils;

namespace BimAiAssistant;

public sealed class App : IExternalApplication
{
    public Result OnStartup(UIControlledApplication application)
    {
        const string tabName = "AI Tools";

        try
        {
            application.CreateRibbonTab(tabName);
        }
        catch
        {
            // Revit throws when the tab already exists after hot reload/restart.
        }

        RibbonPanel panel = application.CreateRibbonPanel(tabName, "BIM Assistant");
        AddButton<ModelStatisticsCommand>(panel, "Model\nStatistics", "Counts walls, doors, windows, floors and rooms.");
        AddButton<AICommand>(panel, "AI\nAssistant", "Ask questions about the active BIM model.");
        AddButton<ExportCommand>(panel, "Export\nModel", "Export model data to building.json.");
        AddButton<FindErrorsCommand>(panel, "Find\nErrors", "Find rooms, doors, walls and windows with missing data.");
        AddButton<GenerateReportCommand>(panel, "Generate\nReport", "Create a PDF model report.");
        AddButton<SettingsCommand>(panel, "Settings", "Configure API URL, key, model and temperature.");

        return Result.Succeeded;
    }

    public Result OnShutdown(UIControlledApplication application) => Result.Succeeded;

    private static void AddButton<TCommand>(RibbonPanel panel, string text, string tooltip)
        where TCommand : IExternalCommand
    {
        string assembly = typeof(App).Assembly.Location;
        var data = new PushButtonData(typeof(TCommand).Name, text, assembly, typeof(TCommand).FullName!)
        {
            ToolTip = tooltip,
            LongDescription = tooltip
        };

        panel.AddItem(data);
    }
}

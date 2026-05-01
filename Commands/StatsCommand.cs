using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BimAiAssistant.Services;

namespace BimAiAssistant.Commands;

public sealed class ModelStatisticsCommand : CommandBase
{
    protected override Result Execute(ExternalCommandData commandData, Document document, ref string message)
    {
        var stats = new RevitService(document).GetStatistics();

        TaskDialog.Show("Model Statistics",
            $"Walls: {stats.Walls}{Environment.NewLine}" +
            $"Doors: {stats.Doors}{Environment.NewLine}" +
            $"Windows: {stats.Windows}{Environment.NewLine}" +
            $"Floors: {stats.Floors}{Environment.NewLine}" +
            $"Rooms: {stats.Rooms}");

        return Result.Succeeded;
    }
}

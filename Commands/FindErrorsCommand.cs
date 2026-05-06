using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BimAiAssistant.Services;

namespace BimAiAssistant.Commands;

public sealed class FindErrorsCommand : CommandBase
{
    protected override Result Execute(ExternalCommandData commandData, Document document, ref string message)
    {
        var errors = new ValidationService(document, new RuleEngineService()).Validate();

        if (errors.Count == 0)
        {
            TaskDialog.Show("Find Errors", "No model errors found.");
            return Result.Succeeded;
        }

        string report = string.Join(Environment.NewLine + Environment.NewLine,
            errors.Select(error => $"{error.Severity} {error.RuleId}{Environment.NewLine}{error.ElementType} #{error.ElementId}{Environment.NewLine}{error.Message}"));

        TaskDialog.Show("Find Errors", report);
        return Result.Succeeded;
    }
}

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace BimAiAssistant.Commands;

[Transaction(TransactionMode.Manual)]
public abstract class CommandBase : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        try
        {
            Document? document = commandData.Application.ActiveUIDocument?.Document;
            if (document is null)
            {
                message = "No active Revit document.";
                return Result.Failed;
            }

            return Execute(commandData, document, ref message);
        }
        catch (Exception ex)
        {
            message = ex.Message;
            TaskDialog.Show("BIM AI Assistant", ex.Message);
            return Result.Failed;
        }
    }

    protected abstract Result Execute(ExternalCommandData commandData, Document document, ref string message);
}

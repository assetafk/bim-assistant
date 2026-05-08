using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BimAiAssistant.Services;

namespace BimAiAssistant.Commands;

public sealed class UndoCommand : CommandBase
{
    protected override Result Execute(ExternalCommandData commandData, Document document, ref string message)
    {
        var result = new UndoService(document, new ChangeHistoryService()).UndoLast();
        TaskDialog.Show("Undo", result.Message);
        return result.Succeeded ? Result.Succeeded : Result.Cancelled;
    }
}

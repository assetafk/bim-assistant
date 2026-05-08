using Autodesk.Revit.DB;
using BimAiAssistant.Models;

namespace BimAiAssistant.Services;

public sealed class UndoService
{
    private readonly Document _document;
    private readonly ChangeHistoryService _changeHistoryService;

    public UndoService(Document document, ChangeHistoryService changeHistoryService)
    {
        _document = document;
        _changeHistoryService = changeHistoryService;
    }

    public AIActionResult UndoLast()
    {
        ChangeOperation? operation = _changeHistoryService.Load()
            .Where(item => !item.IsUndone)
            .OrderByDescending(item => item.CreatedAt)
            .FirstOrDefault();

        if (operation is null)
        {
            return new AIActionResult
            {
                Succeeded = false,
                Message = "No operation available for undo."
            };
        }

        int reverted = 0;
        using var transaction = new Transaction(_document, $"Maybeworks Undo: {operation.ActionName}");
        transaction.Start();

        foreach (ChangeRecord change in operation.Changes.AsEnumerable().Reverse())
        {
            Element? element = _document.GetElement(new ElementId(change.ElementId));
            Parameter? parameter = element?.LookupParameter(change.ParameterName);
            if (parameter is not null && !parameter.IsReadOnly)
            {
                parameter.Set(change.OldValue);
                reverted++;
            }
        }

        transaction.Commit();
        _changeHistoryService.MarkUndone(operation.OperationId);

        return new AIActionResult
        {
            Succeeded = true,
            OperationId = operation.OperationId,
            AffectedElements = reverted,
            Message = $"Undone operation {operation.OperationId}. Reverted {reverted} change(s)."
        };
    }
}

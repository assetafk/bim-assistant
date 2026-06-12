namespace BimAiAssistant.Application.Abstractions;

public interface IValidator<in TModel>
{
    IReadOnlyList<string> Validate(TModel model);
}

namespace Villsource.Modules.Elsa.Contracts.Dtos;

public readonly record struct StartWorkflowResult
{
    public string? InstantId { get; init; }
    public string? ErrorMessage { get; init; }

    public StartWorkflowResult() { }

    public static StartWorkflowResult Success(string instantId) => new() { InstantId = instantId };
    public static StartWorkflowResult Error(string message) => new() { ErrorMessage = message };
};
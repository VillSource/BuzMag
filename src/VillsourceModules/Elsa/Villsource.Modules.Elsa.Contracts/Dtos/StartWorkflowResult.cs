namespace Villsource.Modules.Elsa.Contracts.Dtos;

public readonly record struct StartWorkflowResult
{
    public string? InstantId { get; init; }
    public string? ErrorMessage { get; init; }
    public Exception? InnerException { get; init; }

    public StartWorkflowResult() { }

    public static StartWorkflowResult Success(string instantId) => new() { InstantId = instantId };
    public static StartWorkflowResult Error(string message) => new() { ErrorMessage = message };
    public static StartWorkflowResult Error(Exception exception) => new() { InnerException = exception};

    public static StartWorkflowResult Error(string message, Exception exception) => new()
    {
        ErrorMessage = message, InnerException = exception
    };

    public bool HasError() => ErrorMessage is not null && InnerException is not null;
    public bool IsSuccess() => InstantId is not null;
};
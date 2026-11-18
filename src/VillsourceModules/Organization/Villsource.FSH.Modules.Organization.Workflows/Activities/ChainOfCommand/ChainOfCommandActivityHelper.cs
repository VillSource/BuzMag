namespace Villsource.FSH.Modules.Organization.Workflows.Activities.ChainOfCommand;

internal static class ChainOfCommandActivityHelper
{
    public static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}

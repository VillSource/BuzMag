using Elsa.Workflows.UIHints.Dropdown;
using System.Reflection;

namespace Villsource.Modules.Elsa.Core.Activities.OptionsProviders;

public class ComparisonOperatorOptionsProvider:DropDownOptionsProviderBase
{
    protected override ValueTask<ICollection<SelectListItem>> GetItemsAsync(PropertyInfo propertyInfo, object? context, CancellationToken cancellationToken)
    {
        return ValueTask.FromResult<ICollection<SelectListItem>>([
            new SelectListItem("Equal","=="),
            new SelectListItem("Not Equal","!="),
            new SelectListItem("Greater Than",">="),
            new SelectListItem("Less Than", "<"),
            new SelectListItem("Greater Than or Equal", ">="),
            new SelectListItem("Less Than or Equal", ">="),
        ]);
    }
}
using System.Reflection;
using Elsa.Workflows.UIHints.Dropdown;
using Villsource.FSH.Modules.Organization.Contracts.Constants;

namespace Villsource.FSH.Modules.Organization.Workflows.Activities.OptionsProviders;

public class JobLevelCategoryOptionsProvider : DropDownOptionsProviderBase
{
    protected override ValueTask<ICollection<SelectListItem>> GetItemsAsync(
        PropertyInfo propertyInfo,
        object? context,
        CancellationToken cancellationToken)
    {
        List<SelectListItem> items =
        [
            new SelectListItem("Not Specified", string.Empty)
        ];

        items.AddRange(JobLevelCategory.Items
            .OrderBy(static c => c.SortOrder)
            .Select(static c => new SelectListItem($"({c.SortOrder}) {c.DisplayName} ({c.Key})", c.Key)));

        return ValueTask.FromResult<ICollection<SelectListItem>>(items);
    }
}

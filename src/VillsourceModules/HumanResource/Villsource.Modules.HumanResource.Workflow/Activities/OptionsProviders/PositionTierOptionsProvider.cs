using Elsa.Workflows.UIHints.Dropdown;
using System.Reflection;
using Villsource.FSH.Modules.Organization.Contracts.Constants;

namespace Villsource.Modules.HumanResource.Workflow.Activities.OptionsProviders;

public class PositionTierOptionsProvider : DropDownOptionsProviderBase
{
    protected override ValueTask<ICollection<SelectListItem>> GetItemsAsync(
        PropertyInfo propertyInfo, 
        object? context, 
        CancellationToken cancellationToken)
    {
        ICollection<SelectListItem> items = PositionTier.Items
            .Select(tier => new SelectListItem($"({tier.Tier}) {tier.Key}", tier.Key))
            .ToList();
        
        return ValueTask.FromResult(items);
    }
}
using System.Reflection;
using Elsa.Workflows.UIHints.Dropdown;
using Villsource.FSH.Modules.Organization.Workflows.Constants;

namespace Villsource.FSH.Modules.Organization.Workflows.Activities.OptionsProviders;

public class DelegationTypeOptionsProvider : DropDownOptionsProviderBase
{
    protected override ValueTask<ICollection<SelectListItem>> GetItemsAsync(
        PropertyInfo propertyInfo,
        object? context,
        CancellationToken cancellationToken)
    {
        ICollection<SelectListItem> items =
        [
            new SelectListItem("All Delegation Types", string.Empty),
            new SelectListItem("Full Acting Authority (ACTING_FULL)", OrganizationActivityConstants.DelegationActingFull),
            new SelectListItem("All Document Approvals (APPROVAL_ONLY)", OrganizationActivityConstants.DelegationApprovalOnly),
            new SelectListItem("Specific Document Types Only (SPECIFIC_DOCUMENTS)", OrganizationActivityConstants.DelegationSpecificDocuments),
        ];

        return ValueTask.FromResult(items);
    }
}

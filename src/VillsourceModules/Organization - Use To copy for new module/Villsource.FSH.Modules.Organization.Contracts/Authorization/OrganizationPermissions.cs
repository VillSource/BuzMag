using FSH.Framework.Shared.Constants;

namespace Villsource.FSH.Modules.Organization.Contracts.Authorization;

/// <summary>
/// Permission constants for the Notifications module. Permissions follow the
/// <c>Permissions.{Resource}.{Action}</c> shape per framework convention.
/// </summary>
public static class OrganizationPermissions
{
    public static class Structures
    {
        public const string Resource = "Organization.Structur";
        public const string View = $"Permissions.{Resource}.View";
        public const string Create = $"Permissions.{Resource}.Create";
        public const string Update = $"Permissions.{Resource}.Update";
        public const string Delete = $"Permissions.{Resource}.Delete";
    }
    
    public static class  Charts
    {
        public const string Resource = "Organization.Chart";
        public const string View = $"Permissions.{Resource}.View";
        public const string Create = $"Permissions.{Resource}.Create";
        public const string Update = $"Permissions.{Resource}.Update";
        public const string Delete = $"Permissions.{Resource}.Delete";
    }

    public static IReadOnlyList<FshPermission> All { get; } =
    [
        new("View Organization Structures", ActionConstants.View, Structures.Resource, IsBasic: true),
        new("Create Organization Structures", ActionConstants.Create, Structures.Resource, IsBasic: false),
        new("Update Organization Structures", ActionConstants.Update, Structures.Resource, IsBasic: false),
        new("Delete Organization Structures", ActionConstants.Delete, Structures.Resource, IsBasic: false),
        
        new("View Organization Charts", ActionConstants.View, Charts.Resource, IsBasic: true),
        new("Create Organization Charts", ActionConstants.Create, Charts.Resource, IsBasic: false),
        new("Update Organization Charts", ActionConstants.Update, Charts.Resource, IsBasic: false),
        new("Delete Organization Charts", ActionConstants.Delete, Charts.Resource, IsBasic: false),
    ];
}
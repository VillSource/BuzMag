using FSH.Framework.Shared.Constants;

namespace Villsource.FSH.Modules.Organization.Contracts.Authorization;

/// <summary>
/// Permission constants for the Notifications module. Permissions follow the
/// <c>Permissions.{Resource}.{Action}</c> shape per framework convention.
/// </summary>
public static class OrganizationPermissions
{
    public static class Units
    {
        public const string Resource = "Organization.Unit";
        public const string View = $"Permissions.{Resource}.View";
        public const string Create = $"Permissions.{Resource}.Create";
        public const string Update = $"Permissions.{Resource}.Update";
        public const string Delete = $"Permissions.{Resource}.Delete";
        public const string Move = $"Permissions.{Resource}.Move";
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
        new("View Organization Unit", ActionConstants.View, Units.Resource, IsBasic: true),
        new("Create Organization Unit", ActionConstants.Create, Units.Resource, IsBasic: false),
        new("Update Organization Unit", ActionConstants.Update, Units.Resource, IsBasic: false),
        new("Delete Organization Unit", ActionConstants.Delete, Units.Resource, IsBasic: false),
        new("Move Organization Unit", "Move", Units.Resource, IsBasic: false),
        
        new("View Organization Charts", ActionConstants.View, Charts.Resource, IsBasic: true),
        new("Create Organization Charts", ActionConstants.Create, Charts.Resource, IsBasic: false),
        new("Update Organization Charts", ActionConstants.Update, Charts.Resource, IsBasic: false),
        new("Delete Organization Charts", ActionConstants.Delete, Charts.Resource, IsBasic: false),
    ];
}
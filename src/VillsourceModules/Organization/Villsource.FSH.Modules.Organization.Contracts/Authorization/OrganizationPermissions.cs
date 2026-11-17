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

    public static class UnitTypes
    {
        public const string Resource = "Organization.UnitType";
        public const string View = $"Permissions.{Resource}.View";
        public const string Create = $"Permissions.{Resource}.Create";
        public const string Update = $"Permissions.{Resource}.Update";
        public const string Delete = $"Permissions.{Resource}.Delete";
    }

    public static class JobFamilies
    {
        public const string Resource = "Organization.JobFamily";
        public const string View = $"Permissions.{Resource}.View";
        public const string Create = $"Permissions.{Resource}.Create";
        public const string Update = $"Permissions.{Resource}.Update";
        public const string Delete = $"Permissions.{Resource}.Delete";
    }

    public static class DocumentTypes
    {
        public const string Resource = "Organization.DocumentType";
        public const string View = $"Permissions.{Resource}.View";
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

        new("View Organization Unit Types", ActionConstants.View, UnitTypes.Resource, IsBasic: true),
        new("Create Organization Unit Types", ActionConstants.Create, UnitTypes.Resource, IsBasic: false),
        new("Update Organization Unit Types", ActionConstants.Update, UnitTypes.Resource, IsBasic: false),
        new("Delete Organization Unit Types", ActionConstants.Delete, UnitTypes.Resource, IsBasic: false),

        new("View Job Families", ActionConstants.View, JobFamilies.Resource, IsBasic: true),
        new("Create Job Families", ActionConstants.Create, JobFamilies.Resource, IsBasic: false),
        new("Update Job Families", ActionConstants.Update, JobFamilies.Resource, IsBasic: false),
        new("Delete Job Families", ActionConstants.Delete, JobFamilies.Resource, IsBasic: false),

        new("View Document Types", ActionConstants.View, DocumentTypes.Resource, IsBasic: true),
    ];
}
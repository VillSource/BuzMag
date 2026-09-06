using FSH.Framework.Shared.Constants;

namespace Villsource.Modules.HumanResource.Contracts.Authorization;

public static class HumanResourcePermissions
{
    public static class Employees
    {
        public const string Resource = "HumanResource.Employees";
        
        public const string View = $"Permissions.{Resource}.View";
        public const string Create = $"Permissions.{Resource}.Create";
        public const string Update = $"Permissions.{Resource}.Update";
        public const string Delete = $"Permissions.{Resource}.Delete";
        
        public const string AssignPosition = $"Permissions.{Resource}.AssignPosition";
        public const string UpdateEmployment = $"Permissions.{Resource}.UpdateEmployment";
        public const string Terminate = $"Permissions.{Resource}.Terminate";
    }

    public static IReadOnlyList<FshPermission> All { get; } =
    [
        new("View HumanResource Employees", ActionConstants.View, Employees.Resource, IsBasic: false),
        new("Create HumanResource Employees", ActionConstants.Create, Employees.Resource, IsBasic: false),
        new("Update HumanResource Employees", ActionConstants.Update, Employees.Resource, IsBasic: false),
        new("Delete HumanResource Employees", ActionConstants.Delete, Employees.Resource, IsBasic: false),
        
        new("Assign Position to Employees", ActionConstants.Update, Employees.Resource, IsBasic: false),
        new("Update Employment Status/Type", ActionConstants.Update, Employees.Resource, IsBasic: false),
        new("Terminate Employee Employment", ActionConstants.Delete, Employees.Resource, IsBasic: false),
    ];
}
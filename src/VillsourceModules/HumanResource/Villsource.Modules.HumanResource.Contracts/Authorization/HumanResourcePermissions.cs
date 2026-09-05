using FSH.Framework.Shared.Constants;

namespace Villsource.Modules.HumanResource.Contracts.Authorization;

public static class HumanResourcePermissions
{
    public static class Basic
    {
        public const string Resource = "HumanResource.Basic";
        public const string View = $"Permissions.{Resource}.View";
        public const string Create = $"Permissions.{Resource}.Create";
        public const string Update = $"Permissions.{Resource}.Update";
        public const string Delete = $"Permissions.{Resource}.Delete";
    }
    
    public static IReadOnlyList<FshPermission> All { get; } =
    [
        new("View HumanResource Basic", ActionConstants.View, Basic.Resource, IsBasic: false),
        new("Create HumanResource Basic", ActionConstants.Create, Basic.Resource, IsBasic: false),
        new("Update HumanResource Basic", ActionConstants.Update, Basic.Resource, IsBasic: false),
        new("Delete HumanResource Basic", ActionConstants.Delete, Basic.Resource, IsBasic: false),
    ];
}
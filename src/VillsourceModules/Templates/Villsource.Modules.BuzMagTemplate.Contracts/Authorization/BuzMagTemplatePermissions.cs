using FSH.Framework.Shared.Constants;

namespace Villsource.Modules.BuzMagTemplate.Contracts.Authorization;

public static class BuzMagTemplatePermissions
{
    public static class Basic
    {
        public const string Resource = "BuzMagTemplate.Structur";
        public const string View = $"Permissions.{Resource}.View";
        public const string Create = $"Permissions.{Resource}.Create";
        public const string Update = $"Permissions.{Resource}.Update";
        public const string Delete = $"Permissions.{Resource}.Delete";
    }
    
    public static IReadOnlyList<FshPermission> All { get; } =
    [
        new("View BuzMagTemplate Structures", ActionConstants.View, Basic.Resource, IsBasic: true),
        new("Create BuzMagTemplate Structures", ActionConstants.Create, Basic.Resource, IsBasic: false),
        new("Update BuzMagTemplate Structures", ActionConstants.Update, Basic.Resource, IsBasic: false),
        new("Delete BuzMagTemplate Structures", ActionConstants.Delete, Basic.Resource, IsBasic: false),
    ];
}
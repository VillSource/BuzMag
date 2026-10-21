using FSH.Framework.Shared.Constants;

namespace Villsource.Modules.Oidc.Contracts.Authorization;

public static class OidcPermissions
{
    public static class Basic
    {
        public const string Resource = "Oidc.Structur";
        public const string View = $"Permissions.{Resource}.View";
        public const string Create = $"Permissions.{Resource}.Create";
        public const string Update = $"Permissions.{Resource}.Update";
        public const string Delete = $"Permissions.{Resource}.Delete";
    }

    public static class Applications
    {
        public const string Resource = "Oidc.Applications";
        public const string View = $"Permissions.{Resource}.View";
        public const string Create = $"Permissions.{Resource}.Create";
        public const string Update = $"Permissions.{Resource}.Update";
        public const string Delete = $"Permissions.{Resource}.Delete";
    }

    public static IReadOnlyList<FshPermission> All { get; } =
    [
        new("View Oidc Structures", ActionConstants.View, Basic.Resource, IsBasic: true),
        new("Create Oidc Structures", ActionConstants.Create, Basic.Resource, IsBasic: false),
        new("Update Oidc Structures", ActionConstants.Update, Basic.Resource, IsBasic: false),
        new("Delete Oidc Structures", ActionConstants.Delete, Basic.Resource, IsBasic: false),

        new("View Oidc Applications", ActionConstants.View, Applications.Resource, IsBasic: true),
        new("Create Oidc Applications", ActionConstants.Create, Applications.Resource, IsBasic: false),
        new("Update Oidc Applications", ActionConstants.Update, Applications.Resource, IsBasic: false),
        new("Delete Oidc Applications", ActionConstants.Delete, Applications.Resource, IsBasic: false),
    ];
}
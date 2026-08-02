using Mediator;

namespace Villsource.FSH.Modules.Organization.Contracts.v1.Structures;

public sealed class CreateOrganizationUnit() : ICommand;
public sealed class CreatePosition() : ICommand;

public sealed class UpdateOrganization() : ICommand;
public sealed class UpdateOrganizationUnit() : ICommand;
public sealed class UpdatePosition() : ICommand;

public sealed class DeleteOrganizationUnit() : ICommand;
public sealed class DeletePosition() : ICommand;


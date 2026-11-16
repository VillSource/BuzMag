namespace Villsource.FSH.Modules.Organization.Contracts.v1.PositionDelegations.Dtos;

/// <summary>
/// Active delegation projection resolved for a target position and optional document type at runtime.
/// Consumed directly by the Document Approval workflow engine.
/// </summary>
public sealed record EffectiveDelegationDto(
    string RefId,
    string DelegationType,
    string? DocumentTypeScope,
    DateTimeOffset ValidFrom,
    DateTimeOffset ValidTo,
    DelegationPositionDto DelegatePosition);
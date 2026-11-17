namespace Villsource.FSH.Modules.Organization.Contracts.v1.Positions.Dtos;

public sealed record ApprovalNodeDto(
    int StepOrder,
    string PositionRefId,
    string PositionCode,
    string PositionTitle,
    bool IsVacantOrInactive,
    bool IsDelegated,
    string? EffectiveApproverPositionRefId,
    string? EffectiveApproverPositionCode,
    string? EffectiveApproverPositionTitle,
    string? DelegationRefId,
    string? DelegationReason);

public sealed record ApprovalChainDto(
    string TargetPositionRefId,
    string TargetPositionCode,
    string TargetPositionTitle,
    DateTimeOffset EvaluatedAtUtc,
    string? DocumentTypeCode,
    IReadOnlyList<ApprovalNodeDto> Nodes);

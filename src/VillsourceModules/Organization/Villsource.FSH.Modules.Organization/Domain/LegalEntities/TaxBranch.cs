using FSH.Framework.Core.Domain;
using Villsource.ObjectValue;

namespace Villsource.FSH.Modules.Organization.Domain.LegalEntities;

public sealed partial class TaxBranch : BaseEntity<Guid>, IAuditableEntity, ISoftDeletable
{
    private TaxBranch() { }

    public Guid LegalEntityId { get; private set; }

    public string BranchCode { get; private set; } = null!;
    public bool IsHeadOffice { get; private set; }

    public string Name { get; private set; } = null!;
    public string InternationalName { get; private set; } = null!;
    public string? TaxSubId { get; private set; }

    public Address RegisteredAddress { get; private set; } = null!;

    public bool IsActive { get; private set; } = true;
    public DateTimeOffset? RegisteredDate { get; private set; }
    public DateTimeOffset? ClosedDate { get; private set; }

    internal static TaxBranch Create(
        Guid legalEntityId,
        string branchCode,
        bool isHeadOffice,
        string name,
        string internationalName,
        string? taxSubId,
        Address registeredAddress,
        DateTimeOffset? registeredDate,
        bool isActive)
    {
        return new TaxBranch
        {
            Id = Guid.CreateVersion7(),
            LegalEntityId = legalEntityId,
            BranchCode = branchCode,
            IsHeadOffice = isHeadOffice,
            Name = name,
            InternationalName = internationalName,
            TaxSubId = taxSubId,
            RegisteredAddress = registeredAddress,
            RegisteredDate = registeredDate,
            IsActive = isActive,
            ClosedDate = isActive ? null : DateTimeOffset.UtcNow
        };
    }

    internal void Update(
        string name,
        string internationalName,
        string? taxSubId,
        Address registeredAddress,
        DateTimeOffset? registeredDate)
    {
        Name = name;
        InternationalName = internationalName;
        TaxSubId = taxSubId;
        RegisteredAddress = registeredAddress;
        RegisteredDate = registeredDate;
    }

    internal void Activate()
    {
        IsActive = true;
        ClosedDate = null;
    }

    internal void Close(DateTimeOffset closedDate)
    {
        IsActive = false;
        ClosedDate = closedDate;
    }

    internal void Delete(string? deletedBy, DateTimeOffset deletedOnUtc)
    {
        if (IsDeleted)
        {
            return;
        }

        DeletedOnUtc = deletedOnUtc;
        DeletedBy = deletedBy;
        IsDeleted = true;
        IsActive = false;
    }
}
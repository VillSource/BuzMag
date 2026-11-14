using FSH.Framework.Core.Domain;
using Villsource.FSH.Modules.Organization.Domain.LegalEntities.Events;
using Villsource.ObjectValue;
using Villsource.Tool.UniqueKey;

namespace Villsource.FSH.Modules.Organization.Domain.LegalEntities;

public sealed partial class LegalEntity : BaseEntity<Guid>, IAuditableEntity, ISoftDeletable
{
    private readonly List<TaxBranch> _branches = [];

    private LegalEntity() { }

    public string ReferenceId { get; } = VillsourceId.Key;

    public string Code { get; private set; } = null!;
    public string CountryCode { get; private set; } = null!;
    public string LegalName { get; private set; } = null!;
    public string InternationalName { get; private set; } = null!;
    public string TaxId { get; private set; } = null!;
    public string? RegistrationNumber { get; private set; }
    public string BaseCurrency { get; private set; } = null!;

    public CompanyProfile Profile { get; private set; } = new();
    public IReadOnlyCollection<TaxBranch> Branches => _branches.AsReadOnly();

    public static LegalEntity Create(
        string code,
        string countryCode,
        string legalName,
        string internationalName,
        string taxId,
        string baseCurrency,
        string? registrationNumber = null,
        CompanyProfile? profile = null,
        Address? headOfficeAddress = null,
        string? headOfficeBranchCode = null,
        string? headOfficeName = null,
        string? headOfficeInternationalName = null,
        DateTimeOffset? headOfficeRegisteredDate = null)
    {
        var normalizedCode = NormalizeCode(code: code);
        var normalizedCountry = NormalizeAndValidateCountryCode(countryCode: countryCode);
        var normalizedTaxId = JurisdictionRules.NormalizeAndValidateTaxId(
            countryCode: normalizedCountry,
            taxId: taxId);
        var normalizedRegNo = NormalizeOptionalCode(value: registrationNumber);
        var normalizedCurrency = NormalizeAndValidateCurrency(currency: baseCurrency);

        ArgumentException.ThrowIfNullOrWhiteSpace(legalName);
        ArgumentException.ThrowIfNullOrWhiteSpace(internationalName);

        var entity = new LegalEntity
        {
            Id = Guid.CreateVersion7(),
            Code = normalizedCode,
            CountryCode = normalizedCountry,
            LegalName = legalName.Trim(),
            InternationalName = internationalName.Trim(),
            TaxId = normalizedTaxId,
            RegistrationNumber = normalizedRegNo,
            BaseCurrency = normalizedCurrency,
            Profile = profile is not null ? CloneProfile(profile: profile) : new CompanyProfile()
        };

        entity.AddLegalEntityCreatedDomainEvent();
        if (headOfficeAddress is not null)
        {
            var defaultHoName = string.IsNullOrWhiteSpace(headOfficeName) ? null : headOfficeName.Trim();
            var defaultHoCode = headOfficeBranchCode ??
                                JurisdictionRules.GetDefaultHeadOfficeBranchCode(countryCode: normalizedCountry);
            var defaultHoIntlName = string.IsNullOrWhiteSpace(headOfficeInternationalName)
                ? "Head Office"
                : headOfficeInternationalName.Trim();

            entity.AddBranch(
                branchCode: defaultHoCode,
                name: defaultHoName ?? (normalizedCountry == "TH" ? "สำนักงานใหญ่" : "Head Office"),
                internationalName: defaultHoIntlName,
                registeredAddress: headOfficeAddress,
                isHeadOffice: true,
                registeredDate: headOfficeRegisteredDate);
        }

        return entity;
    }

    public void Update(
        string code,
        string legalName,
        string internationalName,
        string taxId,
        string baseCurrency,
        string? registrationNumber = null)
    {
        EnsureNotDeleted();

        var normalizedCode = NormalizeCode(code: code);
        var normalizedTaxId = JurisdictionRules.NormalizeAndValidateTaxId(
            countryCode: CountryCode,
            taxId: taxId);
        var normalizedRegNo = NormalizeOptionalCode(value: registrationNumber);
        var normalizedCurrency = NormalizeAndValidateCurrency(currency: baseCurrency);

        ArgumentException.ThrowIfNullOrWhiteSpace(legalName);
        ArgumentException.ThrowIfNullOrWhiteSpace(internationalName);

        Code = normalizedCode;
        LegalName = legalName.Trim();
        InternationalName = internationalName.Trim();
        TaxId = normalizedTaxId;
        RegistrationNumber = normalizedRegNo;
        BaseCurrency = normalizedCurrency;

        AddLegalEntityUpdatedDomainEvent();
    }

    public void UpdateProfile(CompanyProfile profile)
    {
        EnsureNotDeleted();
        ArgumentNullException.ThrowIfNull(profile);

        Profile = CloneProfile(profile: profile);
        AddLegalEntityProfileUpdatedDomainEvent();
    }

    public void Delete(string? deletedBy = null)
    {
        if (IsDeleted)
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;
        DeletedOnUtc = now;
        DeletedBy = deletedBy;
        IsDeleted = true;

        foreach (var branch in _branches.Where(b => !b.IsDeleted))
        {
            branch.Delete(deletedBy: deletedBy, deletedOnUtc: now);
            AddTaxBranchDeletedDomainEvent(taxBranchId: branch.Id, branchCode: branch.BranchCode);
        }

        AddLegalEntityDeletedDomainEvent();
    }

    #region TaxBranch / Establishment Aggregate Operations

    public TaxBranch AddBranch(
        string branchCode,
        string name,
        string internationalName,
        Address registeredAddress,
        bool isHeadOffice = false,
        string? taxSubId = null,
        DateTimeOffset? registeredDate = null,
        bool isActive = true)
    {
        EnsureNotDeleted();

        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(internationalName);
        ArgumentNullException.ThrowIfNull(registeredAddress);

        var (normalizedBranchCode, resolvedIsHeadOffice) = JurisdictionRules.NormalizeAndValidateBranch(
            countryCode: CountryCode,
            branchCode: branchCode,
            isHeadOffice: isHeadOffice);

        if (_branches.Any(b => !b.IsDeleted && b.BranchCode == normalizedBranchCode))
        {
            throw new InvalidOperationException(
                $"Branch code '{normalizedBranchCode}' already exists in this legal entity.");
        }

        if (resolvedIsHeadOffice && _branches.Any(b => !b.IsDeleted && b.IsHeadOffice))
        {
            throw new InvalidOperationException("A head office branch already exists in this legal entity.");
        }

        var branch = TaxBranch.Create(
            legalEntityId: Id,
            branchCode: normalizedBranchCode,
            isHeadOffice: resolvedIsHeadOffice,
            name: name.Trim(),
            internationalName: internationalName.Trim(),
            taxSubId: NormalizeOptionalCode(value: taxSubId),
            registeredAddress: registeredAddress,
            registeredDate: registeredDate,
            isActive: isActive);

        _branches.Add(branch);
        AddTaxBranchAddedDomainEvent(branch: branch);

        return branch;
    }

    public void UpdateBranch(
        string branchRef,
        string name,
        string internationalName,
        Address registeredAddress,
        string? taxSubId = null,
        DateTimeOffset? registeredDate = null)
    {
        EnsureNotDeleted();
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(internationalName);
        ArgumentNullException.ThrowIfNull(registeredAddress);

        var branch = GetActiveBranchOrThrow(branchRef);
        branch.Update(
            name: name.Trim(),
            internationalName: internationalName.Trim(),
            taxSubId: NormalizeOptionalCode(value: taxSubId),
            registeredAddress: registeredAddress,
            registeredDate: registeredDate);

        AddTaxBranchUpdatedDomainEvent(branch: branch);
    }
    
    public void ActivateBranch(string branchRef)
    {
        EnsureNotDeleted();

        var branch = GetActiveBranchOrThrow(branchRef);
        if (branch.IsActive)
        {
            return;
        }

        branch.Activate();
        AddTaxBranchActivatedDomainEvent(branch: branch);
    }

    public void CloseBranch(string branchRef, DateTimeOffset? closedDate = null)
    {
        EnsureNotDeleted();

        var branch = GetActiveBranchOrThrow(branchRef);
        if (!branch.IsActive)
        {
            return;
        }

        if (branch.IsHeadOffice && _branches.Any(b => !b.IsDeleted && !b.IsHeadOffice && b.IsActive))
        {
            throw new InvalidOperationException(
                "Cannot close the head office while other active branches still exist.");
        }

        var effectiveClosedDate = closedDate ?? DateTimeOffset.UtcNow;
        branch.Close(closedDate: effectiveClosedDate);

        AddTaxBranchClosedDomainEvent(branch: branch);
    }
    
    public void RemoveBranch(string branchRef, string? deletedBy = null)
    {
        EnsureNotDeleted();

        var branch = GetActiveBranchOrThrow(branchRef);

        if (branch.IsHeadOffice && _branches.Any(b => !b.IsDeleted && !b.IsHeadOffice))
        {
            throw new InvalidOperationException("Cannot delete the head office while other branches still exist.");
        }

        branch.Delete(deletedBy: deletedBy, deletedOnUtc: DateTimeOffset.UtcNow);
        AddTaxBranchDeletedDomainEvent(taxBranchId: branch.Id, branchCode: branch.BranchCode);
    }

    #endregion

    #region Private Helpers & Validations
    
    private TaxBranch GetActiveBranchOrThrow(string branchRef)
    {
        var branch = _branches.FirstOrDefault(b => b.ReferenceId == branchRef && !b.IsDeleted);
        return branch ?? throw new InvalidOperationException($"Tax branch '{branchRef}' was not found.");
    }

    private void EnsureNotDeleted()
    {
        if (IsDeleted)
        {
            throw new InvalidOperationException($"Legal entity '{Id}' has already been deleted.");
        }
    }

    private static CompanyProfile CloneProfile(CompanyProfile profile) => new()
    {
        TradeName = NormalizeOptionalText(value: profile.TradeName),
        LogoUrl = NormalizeOptionalText(value: profile.LogoUrl),
        Website = NormalizeOptionalText(value: profile.Website),
        ContactEmail = NormalizeOptionalText(value: profile.ContactEmail),
        ContactPhone = NormalizeOptionalText(value: profile.ContactPhone)
    };

    private static string NormalizeCode(string code)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        return code.Trim().ToUpperInvariant();
    }

    private static string? NormalizeOptionalCode(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToUpperInvariant();

    private static string? NormalizeOptionalText(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string NormalizeAndValidateCountryCode(string countryCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(countryCode);
        var normalized = countryCode.Trim().ToUpperInvariant();

        if (normalized.Length != 2 || !normalized.All(char.IsAsciiLetterUpper))
        {
            throw new ArgumentException("Country code must be a 2-letter ISO 3166-1 alpha-2 code (e.g., TH, SG, US).",
                nameof(countryCode));
        }

        return normalized;
    }

    private static string NormalizeAndValidateCurrency(string currency)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(currency);
        var normalized = currency.Trim().ToUpperInvariant();

        if (normalized.Length != 3 || !normalized.All(char.IsAsciiLetterUpper))
        {
            throw new ArgumentException(
                "Base currency must be a 3-letter ISO 4217 currency code (e.g., THB, USD, SGD).", nameof(currency));
        }

        return normalized;
    }

    #endregion

    #region Domain Events

    private void AddLegalEntityCreatedDomainEvent() => AddDomainEvent(DomainEvent.Create((id, ts) =>
        new LegalEntityCreatedDomainEvent(
            LegalEntityId: Id,
            Code: Code,
            CountryCode: CountryCode,
            LegalName: LegalName,
            InternationalName: InternationalName,
            TaxId: TaxId,
            RegistrationNumber: RegistrationNumber,
            BaseCurrency: BaseCurrency,
            EventId: id,
            OccurredOnUtc: ts)));

    private void AddLegalEntityUpdatedDomainEvent() => AddDomainEvent(DomainEvent.Create((id, ts) =>
        new LegalEntityUpdatedDomainEvent(
            LegalEntityId: Id,
            Code: Code,
            CountryCode: CountryCode,
            LegalName: LegalName,
            InternationalName: InternationalName,
            TaxId: TaxId,
            RegistrationNumber: RegistrationNumber,
            BaseCurrency: BaseCurrency,
            EventId: id,
            OccurredOnUtc: ts)));

    private void AddLegalEntityProfileUpdatedDomainEvent() => AddDomainEvent(DomainEvent.Create((id, ts) =>
        new LegalEntityProfileUpdatedDomainEvent(
            LegalEntityId: Id,
            TradeName: Profile.TradeName,
            EventId: id,
            OccurredOnUtc: ts)));

    private void AddLegalEntityDeletedDomainEvent() => AddDomainEvent(DomainEvent.Create((id, ts) =>
        new LegalEntityDeletedDomainEvent(
            LegalEntityId: Id,
            EventId: id,
            OccurredOnUtc: ts)));

    private void AddTaxBranchAddedDomainEvent(TaxBranch branch) => AddDomainEvent(DomainEvent.Create((id, ts) =>
        new TaxBranchAddedDomainEvent(
            LegalEntityId: Id,
            TaxBranchId: branch.Id,
            BranchCode: branch.BranchCode,
            IsHeadOffice: branch.IsHeadOffice,
            Name: branch.Name,
            InternationalName: branch.InternationalName,
            EventId: id,
            OccurredOnUtc: ts)));

    private void AddTaxBranchUpdatedDomainEvent(TaxBranch branch) => AddDomainEvent(DomainEvent.Create((id, ts) =>
        new TaxBranchUpdatedDomainEvent(
            LegalEntityId: Id,
            TaxBranchId: branch.Id,
            BranchCode: branch.BranchCode,
            Name: branch.Name,
            InternationalName: branch.InternationalName,
            EventId: id,
            OccurredOnUtc: ts)));

    private void AddTaxBranchActivatedDomainEvent(TaxBranch branch) => AddDomainEvent(DomainEvent.Create((id, ts) =>
        new TaxBranchActivatedDomainEvent(
            LegalEntityId: Id,
            TaxBranchId: branch.Id,
            BranchCode: branch.BranchCode,
            EventId: id,
            OccurredOnUtc: ts)));

    private void AddTaxBranchClosedDomainEvent(TaxBranch branch) => AddDomainEvent(DomainEvent.Create((id, ts) =>
        new TaxBranchClosedDomainEvent(
            LegalEntityId: Id,
            TaxBranchId: branch.Id,
            BranchCode: branch.BranchCode,
            ClosedDate: branch.ClosedDate!.Value,
            EventId: id,
            OccurredOnUtc: ts)));

    private void AddTaxBranchDeletedDomainEvent(Guid taxBranchId, string branchCode) => AddDomainEvent(
        DomainEvent.Create((id, ts) =>
            new TaxBranchDeletedDomainEvent(
                LegalEntityId: Id,
                TaxBranchId: taxBranchId,
                BranchCode: branchCode,
                EventId: id,
                OccurredOnUtc: ts)));

    #endregion
}

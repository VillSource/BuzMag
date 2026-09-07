using FSH.Framework.Core.Domain;
using Villsource.FSH.Modules.Organization.Contracts.Constants;
using Villsource.Modules.HumanResource.Contracts.Constants;
using Villsource.ObjectValue;
using Villsource.Tool.UniqueKey;

namespace Villsource.Modules.HumanResource.Domain;

public sealed partial class Employee : AggregateRoot<Guid>, IAuditableEntity, ISoftDeletable
{
    public string Ref { get; } = VillsourceId.Key;
    public string? UserId { get; private set; }
    public string Code { get; private set; } = string.Empty;

    // personal information
    public string Title { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string? MiddleName { get; private set; }
    public string LastName { get; private set; } = string.Empty;
    public string TitleEn { get; private set; } = string.Empty;
    public string FirstNameEn { get; private set; } = string.Empty;
    public string? MiddleNameEn { get; private set; }
    public string LastNameEn { get; private set; } = string.Empty;
    public Address Address { get; private set; } = Address.Empty();
    public string Email { get; private set; } = string.Empty;


    // job label
    public EmployeeStatus Status { get; private set; } = EmployeeStatus.None; 
    public DateTimeOffset? LastHireDate { get; }
    public DateTimeOffset? FirstHireDate { get; }
    public Guid? SnapshotManagerId { get; }
    public string? SnapshotManagerRef { get; }
    public string? SnapshotOuRef { get; }
    public string? SnapshotPositionRef { get; }
    public PositionTier? SnapshotTier { get; }

    private readonly List<Employment> _employments = [];
    public IReadOnlyCollection<Employment> Employments => _employments.AsReadOnly();

    private readonly List<PositionAssignment> _positionAssignments = [];
    public IReadOnlyCollection<PositionAssignment> PositionAssignments => _positionAssignments.AsReadOnly();


    private Employee()
    {
    }

    public static Employee Create(
        string code, string title, string firstName, string lastName,
        string titleEn, string firstNameEn, string lastNameEn, string email, Address address)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(firstName);
        ArgumentException.ThrowIfNullOrWhiteSpace(lastName);
        ArgumentException.ThrowIfNullOrWhiteSpace(titleEn);
        ArgumentException.ThrowIfNullOrWhiteSpace(firstNameEn);
        ArgumentException.ThrowIfNullOrWhiteSpace(lastNameEn);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        var employee = new Employee
        {
            Code = code,
            Title = title,
            FirstName = firstName,
            LastName = lastName,
            TitleEn = titleEn,
            FirstNameEn = firstNameEn,
            LastNameEn = lastNameEn,
            Email = email,
            Status = EmployeeStatus.Offboarding,
            Address = address
        };

        return employee;
    }

    public void UpdatePersonalInfo(
        string title,
        string firstName,
        string? middleName,
        string lastName,
        string titleEn,
        string firstNameEn,
        string lastNameEn,
        string? middleNameEn,
        Address? address = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(firstName);
        ArgumentException.ThrowIfNullOrWhiteSpace(lastName);
        ArgumentException.ThrowIfNullOrWhiteSpace(titleEn);
        ArgumentException.ThrowIfNullOrWhiteSpace(firstNameEn);
        ArgumentException.ThrowIfNullOrWhiteSpace(lastNameEn);

        Title = title;
        FirstName = firstName;
        LastName = lastName;
        MiddleName = middleName;
        TitleEn = titleEn;
        FirstNameEn = firstNameEn;
        LastNameEn = lastNameEn;
        MiddleNameEn = middleNameEn;
        if (address is not null)
            Address = address;
    }

    public void SetMiddleName(string? middleName, string? middleNameEn)
    {
        MiddleName = middleName;
        MiddleNameEn = middleNameEn;
    }

    public void BindUser(string userId)
    {
        UserId = userId;
    }

    public void UnbindUser()
    {
        UserId = null;
    }

    public void ChangeEmail(string newEmail)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newEmail);
        Email = newEmail;
    }
}
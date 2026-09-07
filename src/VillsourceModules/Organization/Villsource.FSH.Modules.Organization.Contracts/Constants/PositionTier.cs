namespace Villsource.FSH.Modules.Organization.Contracts.Constants;

using Thinktecture;

[SmartEnum<string>]
public partial class PositionTier
{
    public int Tier { get; }
    public static readonly PositionTier None = new("NONE",  0);
    public static readonly PositionTier Trainee = new("TRAINEE", 100);
    public static readonly PositionTier Junior = new("JUNIOR", 100);
    public static readonly PositionTier Staff = new("STAFF", 200);
    public static readonly PositionTier MidTier = new("MIDTIER", 200);
    public static readonly PositionTier Supervisor = new("SUPERVISOR", 200);
    public static readonly PositionTier Senior = new("SENIOR", 200);
    public static readonly PositionTier Manager = new("MANAGER", 200);
    public static readonly PositionTier Lead = new("LEAD", 200);
    public static readonly PositionTier Director = new("DIRECTOR", 200);
    public static readonly PositionTier Principal = new("PRINCIPAL", 200);
    public static readonly PositionTier Executive = new("EXECUTIVE", 200);
    public static readonly PositionTier Distinguished = new("DISTINGUISHED", 200);
}
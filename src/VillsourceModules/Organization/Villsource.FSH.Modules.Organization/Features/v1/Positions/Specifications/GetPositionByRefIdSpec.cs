using FSH.Framework.Persistence.Specifications;
using Position = Villsource.FSH.Modules.Organization.Domain.Positions.Position;

namespace Villsource.FSH.Modules.Organization.Features.v1.Positions.Specifications;

public sealed class GetPositionByRefIdSpec : Specification<Position>
{
    public GetPositionByRefIdSpec(string refId)
    {
        Where(p => p.ReferenceId == refId);
        Include(p => p.LegalEntity!);
        Include(p => p.Job!);
        Include(p => p.JobGrade!);
        Include(p => p.CostCenter!);
        Include("ReportsToPosition.JobGrade");
        Include("DottedLinePosition.JobGrade");
        Include("DirectReports.JobGrade");
    }
}
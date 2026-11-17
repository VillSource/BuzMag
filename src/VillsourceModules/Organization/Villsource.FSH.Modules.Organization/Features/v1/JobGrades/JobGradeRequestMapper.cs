using Riok.Mapperly.Abstractions;
using Villsource.FSH.Modules.Organization.Contracts.v1.JobGrades.Commands;


namespace Villsource.FSH.Modules.Organization.Features.v1.JobGrades
{
    [Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
    internal static partial class JobGradeRequestMapper
    {
        internal static partial UpdateJobGradeCommand ToCommand(this UpdateJobGradeRequest request, string refId);
    }

}
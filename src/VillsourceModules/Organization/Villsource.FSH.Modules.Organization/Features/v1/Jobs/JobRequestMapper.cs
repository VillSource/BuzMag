using Riok.Mapperly.Abstractions;
using Villsource.FSH.Modules.Organization.Contracts.v1.Jobs.Commands;

namespace Villsource.FSH.Modules.Organization.Features.v1.Jobs;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
internal static partial class JobRequestMapper
{
    internal static partial UpdateJobCommand ToCommand(this UpdateJobRequest request, string refId);
}
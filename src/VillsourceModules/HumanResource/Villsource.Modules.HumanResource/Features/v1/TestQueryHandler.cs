using Mediator;
using Villsource.Modules.HumanResource.Contracts.v1;

namespace Villsource.Modules.HumanResource.Features.v1;

public class TestQueryHandler : IQueryHandler<TestQuery, Unit>
{
    public ValueTask<Unit> Handle(TestQuery query, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
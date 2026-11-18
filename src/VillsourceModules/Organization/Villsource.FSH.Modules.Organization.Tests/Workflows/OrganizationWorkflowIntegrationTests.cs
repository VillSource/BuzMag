using System.Text.Json;
using System.Text.Json.Nodes;
using Elsa.Workflows.Activities;
using Elsa.Workflows.Activities.Flowchart.Activities;
using Elsa.Workflows.Activities.Flowchart.Models;
using Elsa.Workflows.Memory;
using Elsa.Workflows.Models;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using FSH.Framework.Shared.Multitenancy;
using FSH.Framework.Shared.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NSubstitute;
using Villsource.FSH.Modules.Organization.Contracts.Constants;
using Villsource.FSH.Modules.Organization.Contracts.Services.Dtos;
using Villsource.FSH.Modules.Organization.Data;
using Villsource.FSH.Modules.Organization.Domain.JobGrades;
using Villsource.FSH.Modules.Organization.Domain.Jobs;
using Villsource.FSH.Modules.Organization.Domain.LegalEntities;
using Villsource.FSH.Modules.Organization.Domain.OrganizationUnits;
using Villsource.FSH.Modules.Organization.Domain.OrganizationUnitTypes;
using OrgPosition = Villsource.FSH.Modules.Organization.Domain.Positions.Position;
using Villsource.FSH.Modules.Organization.Services;
using Villsource.FSH.Modules.Organization.Workflows.Activities.ChainOfCommand;
using Villsource.Modules.Elsa.Core.Activities;

namespace Villsource.FSH.Modules.Organization.Tests.Workflows;

public sealed class OrganizationWorkflowIntegrationTests
{
    private static readonly JsonSerializerOptions WebJsonOptions = new(JsonSerializerDefaults.Web);
    [Fact]
    public async Task EndToEnd_Flowchart_Should_ResolveManager_SelectField_CompareValues_And_Branch()
    {
        await using var db = CreateDbContext();
        var (staffRefId, _) = await SeedHierarchyAsync(db);
        var realService = new ChainOfCommandService(db);

        await using var sp = WorkflowTestHost.BuildServiceProvider(realService);

        var resolvedManager = new Variable<JsonObject?>("resolvedManager", null);
        var approvalLimit = new Variable<decimal?>("approvalLimit", null);
        var rankOrder = new Variable<int?>("rankOrder", null);
        var meetsLimit = new Variable<bool>("meetsLimit", false);
        var outcome = string.Empty;

        var resolveActivity = new ResolveDirectManager
        {
            Id = "resolve-direct-manager",
            PositionRefId = new Input<string>(staffRefId),
            ClimbToFirstActive = new Input<bool>(false),
            Result = new Output<JsonObject?>(resolvedManager)
        };

        var selectFieldActivity = new ChainOfCommandPositionFieldSelector
        {
            Id = "select-limit-and-rank",
            Position = new Input<object?>(resolvedManager),
            ApprovalLimitAmount = new Output<decimal?>(approvalLimit),
            JobGradeRankOrder = new Output<int?>(rankOrder)
        };

        var compareActivity = new CompareValues
        {
            Id = "compare-limit",
            Left = new Input<object?>(ctx => approvalLimit.Get(ctx)),
            Operator = new Input<string>(">="),
            Right = new Input<object?>("50000"),
            Result = new Output<bool>(meetsLimit)
        };

        var decision = new FlowDecision
        {
            Condition = new Input<bool>(ctx => meetsLimit.Get(ctx))
        };

        var approvedBranch = new Inline(_ =>
        {
            outcome = "ManagerCanApprove";
            return ValueTask.CompletedTask;
        });

        var escalateBranch = new Inline(_ =>
        {
            outcome = "NeedsEscalation";
            return ValueTask.CompletedTask;
        });

        var flowchart = new Flowchart
        {
            Variables = { resolvedManager, approvalLimit, rankOrder, meetsLimit },
            Start = resolveActivity,
            Activities =
            {
                resolveActivity,
                selectFieldActivity,
                compareActivity,
                decision,
                approvedBranch,
                escalateBranch
            },
            Connections =
            {
                new Connection(resolveActivity, selectFieldActivity),
                new Connection(selectFieldActivity, compareActivity),
                new Connection(compareActivity, decision),
                new Connection(new Endpoint(decision, "True"), new Endpoint(approvedBranch)),
                new Connection(new Endpoint(decision, "False"), new Endpoint(escalateBranch))
            }
        };

        var runResult = await WorkflowTestHost.RunActivityAsync(sp, flowchart);
        runResult.WorkflowState.Incidents.ShouldBeEmpty();
        WorkflowTestHost.GetOutput<decimal?>(runResult, selectFieldActivity, nameof(ChainOfCommandPositionFieldSelector.ApprovalLimitAmount)).ShouldBe(100_000m);
        WorkflowTestHost.GetOutput<int?>(runResult, selectFieldActivity, nameof(ChainOfCommandPositionFieldSelector.JobGradeRankOrder)).ShouldBe(30);
        WorkflowTestHost.GetOutput<bool>(runResult, compareActivity, "Result").ShouldBeTrue();
        outcome.ShouldBe("ManagerCanApprove");
    }

    [Fact]
    public async Task EndToEnd_ReportingChain_Should_Work_With_ForEach_And_FieldSelector()
    {
        await using var db = CreateDbContext();
        var (staffRefId, _) = await SeedHierarchyAsync(db);
        var realService = new ChainOfCommandService(db);

        await using var sp = WorkflowTestHost.BuildServiceProvider(realService);

        var chainJsonVar = new Variable<JsonObject>("chainJson", new JsonObject());
        var currentPosVar = new Variable<ChainOfCommandPositionDto?>("currentPos", null);
        var currentCodeVar = new Variable<string?>("currentCode", null);
        var collectedCodes = new List<string>();

        var resolveChain = new ResolveReportingChain
        {
            PositionRefId = new Input<string>(staffRefId),
            Result = new Output<JsonObject>(chainJsonVar)
        };

        var loop = new ForEach<ChainOfCommandPositionDto>
        {
            Items = new Input<ICollection<ChainOfCommandPositionDto>>(ctx =>
                chainJsonVar.Get(ctx)?["items"]?.Deserialize<List<ChainOfCommandPositionDto>>(WebJsonOptions) ?? []),
            CurrentValue = new Output<ChainOfCommandPositionDto>(currentPosVar),
            Body = new Sequence
            {
                Activities =
                {
                    new ChainOfCommandPositionFieldSelector
                    {
                        Position = new Input<object?>(currentPosVar),
                        Code = new Output<string?>(currentCodeVar)
                    },
                    new Inline(ctx =>
                    {
                        var code = currentCodeVar.Get(ctx);
                        if (code is not null)
                        {
                            collectedCodes.Add(code);
                        }
                        return ValueTask.CompletedTask;
                    })
                }
            }
        };

        var sequence = new Sequence
        {
            Variables = { chainJsonVar, currentPosVar, currentCodeVar },
            Activities = { resolveChain, loop }
        };

        await WorkflowTestHost.RunActivityAsync(sp, sequence);
        collectedCodes.ShouldBe(["POS-MGR", "POS-CEO"]);
    }

    [Fact]
    public async Task EndToEnd_NonGeneric_ForEach_Should_Accept_Deserialized_Json_List()
    {
        await using var db = CreateDbContext();
        var (staffRefId, _) = await SeedHierarchyAsync(db);
        var realService = new ChainOfCommandService(db);

        await using var sp = WorkflowTestHost.BuildServiceProvider(realService);

        var chainJsonVar = new Variable<JsonObject>("chainJson", new JsonObject());
        var currentItemVar = new Variable<object>("currentItem", new object());
        var selectedRankVar = new Variable<int?>("selectedRank", null);
        var collectedRanks = new List<int>();

        var resolveChain = new ResolveReportingChain
        {
            PositionRefId = new Input<string>(staffRefId),
            Result = new Output<JsonObject>(chainJsonVar)
        };

        var loop = new ForEach
        {
            Items = new Input<ICollection<object>>(ctx =>
                (chainJsonVar.Get(ctx)?["items"]?.AsArray() ?? [])
                .OfType<JsonNode>()
                .Cast<object>()
                .ToList()),
            CurrentValue = new Output<object>(currentItemVar),
            Body = new Sequence
            {
                Activities =
                {
                    new ChainOfCommandPositionFieldSelector
                    {
                        Position = new Input<object?>(currentItemVar),
                        JobGradeRankOrder = new Output<int?>(selectedRankVar)
                    },
                    new Inline(ctx =>
                    {
                        if (selectedRankVar.Get(ctx) is int rank)
                        {
                            collectedRanks.Add(rank);
                        }
                        return ValueTask.CompletedTask;
                    })
                }
            }
        };

        var sequence = new Sequence
        {
            Variables = { chainJsonVar, currentItemVar, selectedRankVar },
            Activities = { resolveChain, loop }
        };

        var runResult = await WorkflowTestHost.RunActivityAsync(sp, sequence);
        runResult.WorkflowState.Incidents.ShouldBeEmpty();
        collectedRanks.ShouldBe([30, 40]);
    }

    private static OrganizationDbContext CreateDbContext()
    {
        var tenantAccessor = new TestTenantAccessor();
        ((IMultiTenantContextSetter)tenantAccessor).MultiTenantContext =
            new MultiTenantContext<AppTenantInfo>(new AppTenantInfo("tenant-wf", "tenant-wf"));

        var options = new DbContextOptionsBuilder<OrganizationDbContext>()
            .UseInMemoryDatabase($"org-wf-{Guid.NewGuid():N}")
            .Options;

        var settings = Options.Create(new DatabaseOptions());
        var environment = Substitute.For<IHostEnvironment>();
        environment.EnvironmentName.Returns("Development");

        return new OrganizationDbContext(tenantAccessor, options, settings, environment);
    }

    private static async Task<(string StaffRefId, string CeoRefId)> SeedHierarchyAsync(OrganizationDbContext db)
    {
        var le = LegalEntity.Create("ACME", "TH", "Acme Thailand", "Acme Thailand", "0105560000001", "THB");
        var ouType = OrganizationUnitType.Create("UNIT", "Unit", "Unit", 1);

        var grade1 = JobGrade.Create("G1", "Grade 1", "Grade 1", 10, jobLevelCategory: JobLevelCategory.Operational);
        var grade3 = JobGrade.Create("G3", "Grade 3", "Grade 3", 30, jobLevelCategory: JobLevelCategory.Management);
        var grade4 = JobGrade.Create("G4", "Grade 4", "Grade 4", 40, jobLevelCategory: JobLevelCategory.Executive);

        var job = Job.Create("ENG", "Engineer", "Engineer", grade1.Id);
        var rootOu = OrganizationUnit.Create("ROOT", "Headquarters", "Headquarters", ouType.Id, legalEntityId: le.Id);
        var deptOu = OrganizationUnit.Create("DEPT", "Engineering", "Engineering", ouType.Id, legalEntityId: le.Id, parent: rootOu);

        var ceo = OrgPosition.Create(
            le.Id, rootOu.Id, job.Id, grade4.Id,
            "POS-CEO", "Chief Executive Officer", "CEO",
            approvalLimitAmount: 1_000_000m, approvalLimitCurrency: "THB", isPrimaryLeadership: true);

        var deptManager = OrgPosition.Create(
            le.Id, deptOu.Id, job.Id, grade3.Id,
            "POS-MGR", "Engineering Manager", "Engineering Manager",
            approvalLimitAmount: 100_000m, approvalLimitCurrency: "THB",
            reportsToPositionId: ceo.Id, isPrimaryLeadership: true);

        var staff = OrgPosition.Create(
            le.Id, deptOu.Id, job.Id, grade1.Id,
            "POS-STAFF", "Software Engineer", "Software Engineer",
            reportsToPositionId: deptManager.Id);

        rootOu.AssignManagerPosition(ceo, allowExternalManagerPosition: true);
        deptOu.AssignManagerPosition(deptManager, allowExternalManagerPosition: true);

        db.LegalEntities.Add(le);
        db.OrganizationUnitTypes.Add(ouType);
        db.JobGrades.AddRange(grade1, grade3, grade4);
        db.Jobs.Add(job);
        db.OrganizationUnits.AddRange(rootOu, deptOu);
        db.Positions.AddRange(ceo, deptManager, staff);

        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        return (staff.ReferenceId, ceo.ReferenceId);
    }

    private sealed class TestTenantAccessor : IMultiTenantContextAccessor<AppTenantInfo>, IMultiTenantContextSetter
    {
        private IMultiTenantContext<AppTenantInfo> _context = new MultiTenantContext<AppTenantInfo>(new AppTenantInfo());

        public IMultiTenantContext<AppTenantInfo> MultiTenantContext => _context;
        IMultiTenantContext IMultiTenantContextAccessor.MultiTenantContext => _context;
        IMultiTenantContext IMultiTenantContextSetter.MultiTenantContext
        {
            set => _context = (IMultiTenantContext<AppTenantInfo>)value;
        }
    }
}

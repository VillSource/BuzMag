using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Villsource.Modules.Elsa.Domain;

namespace Villsource.Modules.Elsa.Data.Configurations;

public class ApprovalInboxConfiguration: IEntityTypeConfiguration<ApprovalInbox>
{
    public void Configure(EntityTypeBuilder<ApprovalInbox> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.ToTable("ApprovalInboxes");
    }
}
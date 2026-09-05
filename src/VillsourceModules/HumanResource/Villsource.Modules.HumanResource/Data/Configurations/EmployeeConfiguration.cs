using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Villsource.Modules.HumanResource.Domain;
using Villsource.ObjectValue;

namespace Villsource.Modules.HumanResource.Data.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        
        builder.ToTable("OrganizationUnits");

        builder.ComplexProperty(e => e.Address)
            .ConfigureAddress();

        builder.HasMany(e => e.Employments)
            .WithOne()
            .HasForeignKey(e => e.EmployeeId)
            .OnDelete(DeleteBehavior.NoAction);
        
        builder.HasMany(e => e.PositionAssignments)
            .WithOne()
            .HasForeignKey(e => e.EmployeeId)
            .OnDelete(DeleteBehavior.NoAction);
        
        builder
            .Navigation(e => e.Employments)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
        
        builder
            .Navigation(e => e.PositionAssignments)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
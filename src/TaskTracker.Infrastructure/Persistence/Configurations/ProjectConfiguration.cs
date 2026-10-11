using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskTracker.Domain.Projects;

namespace TaskTracker.Infrastructure.Persistence.Configurations;

internal sealed class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("Projects");
        builder.HasKey(project => project.Id);

        builder.Property(project => project.Id)
            .HasConversion(id => id.Value, value => new ProjectId(value))
            .ValueGeneratedNever();

        builder.Property(project => project.Name)
            .HasConversion(name => name.Value, value => ProjectName.Create(value))
            .HasMaxLength(ProjectName.MaxLength)
            .IsRequired();

        builder.Property(project => project.Status)
            .HasConversion<string>()
            .HasMaxLength(EnumColumn.MaxLength);

        builder.Property(project => project.CreatedAt);

        builder.HasMany(project => project.Tasks)
            .WithOne()
            .HasForeignKey(task => task.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(project => project.Tasks)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

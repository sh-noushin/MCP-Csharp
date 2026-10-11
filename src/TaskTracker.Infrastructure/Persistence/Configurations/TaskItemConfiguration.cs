using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskTracker.Domain.Projects;
using TaskTracker.Domain.Tasks;

namespace TaskTracker.Infrastructure.Persistence.Configurations;

internal sealed class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        builder.ToTable("Tasks");
        builder.HasKey(task => task.Id);

        builder.Property(task => task.Id)
            .HasConversion(id => id.Value, value => new TaskItemId(value))
            .ValueGeneratedNever();

        builder.Property(task => task.ProjectId)
            .HasConversion(id => id.Value, value => new ProjectId(value));

        builder.Property(task => task.Title)
            .HasConversion(title => title.Value, value => TaskTitle.Create(value))
            .HasMaxLength(TaskTitle.MaxLength)
            .IsRequired();

        builder.Property(task => task.Priority)
            .HasConversion<string>()
            .HasMaxLength(EnumColumn.MaxLength);

        builder.Property(task => task.Status)
            .HasConversion<string>()
            .HasMaxLength(EnumColumn.MaxLength);

        builder.Property(task => task.DueDate);
        builder.Property(task => task.CreatedAt);
        builder.Property(task => task.CompletedAt);
    }
}

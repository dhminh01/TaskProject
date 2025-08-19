using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskService.Domain.Entities;

namespace TaskService.Infrastructure.Persistence.Configurations;

public class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        builder.ToTable("TaskItems");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasColumnName("Id")
            .ValueGeneratedNever();

        builder.Property(t => t.Title)
            .HasColumnName("Title")
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(t => t.Title)
            .IsUnique();

        builder.Property(t => t.Description)
            .HasColumnName("Description")
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(t => t.DueDate)
            .HasColumnName("DueDate")
            .HasColumnType("datetime2");

        builder.Property(t => t.DateCreated)
            .HasColumnName("DateCreated")
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(t => t.DateUpdated)
            .HasColumnName("DateUpdated")
            .HasColumnType("datetime2")
            .IsRequired();
    }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Valora.Domain;

namespace Valora.Infrastructure;

public sealed class ValoraDbContext(DbContextOptions<ValoraDbContext> options) : DbContext(options)
{
    public DbSet<Entry> Entries => Set<Entry>();
    public DbSet<RecurrenceRule> RecurrenceRules => Set<RecurrenceRule>();
    public DbSet<RecurrenceRuleVersion> RecurrenceRuleVersions => Set<RecurrenceRuleVersion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var monthConverter = new ValueConverter<MonthKey, int>(month => month.Year * 100 + month.Month,
            value => new MonthKey(value / 100, value % 100));
        var nullableMonthConverter = new ValueConverter<MonthKey?, int?>(
            month => month.HasValue ? month.Value.Year * 100 + month.Value.Month : null,
            value => value.HasValue ? new MonthKey(value.Value / 100, value.Value % 100) : null);
        var moneyConverter = new ValueConverter<MoneyAmount, decimal>(amount => amount.Value,
            value => new MoneyAmount(value));

        modelBuilder.Entity<Entry>(entity =>
        {
            entity.ToTable("entries");
            entity.HasKey(entry => entry.Id);
            entity.Property(entry => entry.Month).HasConversion(monthConverter).HasColumnName("month_key");
            entity.Property(entry => entry.Kind).HasConversion<string>().HasMaxLength(16).HasColumnName("kind");
            entity.Property(entry => entry.Status).HasConversion<string>().HasMaxLength(24).HasColumnName("status");
            entity.Property(entry => entry.Description).HasMaxLength(300).IsRequired().HasColumnName("description");
            entity.Property(entry => entry.Note).HasMaxLength(2000).HasColumnName("note");
            entity.Property(entry => entry.Amount).HasConversion(moneyConverter).HasColumnType("numeric(31,2)").HasColumnName("amount");
            entity.Property(entry => entry.RecurrenceRuleId).HasColumnName("recurrence_rule_id");
            entity.Property(entry => entry.RecurrenceRuleVersionId).HasColumnName("recurrence_rule_version_id");
            entity.HasIndex(entry => new { entry.RecurrenceRuleId, entry.Month }).IsUnique()
                .HasFilter("\"recurrence_rule_id\" IS NOT NULL");
            entity.HasIndex(entry => entry.Month);
            entity.HasOne<RecurrenceRule>().WithMany().HasForeignKey(entry => entry.RecurrenceRuleId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<RecurrenceRuleVersion>().WithMany().HasForeignKey(entry => entry.RecurrenceRuleVersionId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<RecurrenceRule>(entity =>
        {
            entity.ToTable("recurrence_rules");
            entity.HasKey(rule => rule.Id);
            entity.Property(rule => rule.Kind).HasConversion<string>().HasMaxLength(16).HasColumnName("kind");
            entity.Property(rule => rule.Description).HasMaxLength(300).IsRequired().HasColumnName("description");
            entity.Property(rule => rule.StartMonth).HasConversion(monthConverter).HasColumnName("start_month");
            entity.Property(rule => rule.EndMonth).HasConversion(nullableMonthConverter).HasColumnName("end_month");
            entity.HasMany(rule => rule.Versions).WithOne().HasForeignKey(version => version.RuleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RecurrenceRuleVersion>(entity =>
        {
            entity.ToTable("recurrence_rule_versions");
            entity.HasKey(version => version.Id);
            entity.Property(version => version.EffectiveFromMonth).HasConversion(monthConverter)
                .HasColumnName("effective_from_month");
            entity.Property(version => version.Kind).HasConversion<string>().HasMaxLength(16).HasColumnName("kind");
            entity.Property(version => version.Description).HasMaxLength(300).IsRequired().HasColumnName("description");
            entity.Property(version => version.Status).HasConversion<string>().HasMaxLength(16).HasColumnName("status");
            entity.Property(version => version.ValueMode).HasConversion<string>().HasMaxLength(24).HasColumnName("value_mode");
            entity.Property(version => version.DefaultAmount).HasConversion(moneyConverter).HasColumnType("numeric(31,2)")
                .HasColumnName("default_amount");
            entity.Property(version => version.RuleId).HasColumnName("rule_id");
            entity.HasIndex(version => new { version.RuleId, version.EffectiveFromMonth }).IsUnique();
        });
    }
}

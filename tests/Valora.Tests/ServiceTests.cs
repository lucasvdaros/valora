using Microsoft.EntityFrameworkCore;
using Valora.Application;
using Valora.Domain;
using Valora.Infrastructure;

namespace Valora.Tests;

public sealed class ServiceTests
{
    private static readonly MonthKey Current = new(2026, 11);

    [Fact]
    public async Task CreateRule_CreatesInitialOccurrenceAtomically_AndOpeningMonthIsIdempotent()
    {
        var options = CreateOptions();
        RecurrenceRuleResponse rule;
        await using (var db = new ValoraDbContext(options))
            rule = await new ValoraService(db, new FixedMonthClock(Current), new MonthSummaryCalculator()).CreateRuleAsync(
                new RecurrenceRequest("expense", "Rent", "2026-11", "copy_default", "1800.00"), CancellationToken.None);

        MonthResponse firstOpen;
        MonthResponse secondOpen;
        await using (var db = new ValoraDbContext(options))
            firstOpen = await new ValoraService(db, new FixedMonthClock(Current), new MonthSummaryCalculator()).OpenMonthAsync("2026-11", CancellationToken.None);
        await using (var db = new ValoraDbContext(options))
            secondOpen = await new ValoraService(db, new FixedMonthClock(Current), new MonthSummaryCalculator()).OpenMonthAsync("2026-11", CancellationToken.None);

        Assert.Single(firstOpen.Entries);
        Assert.Single(secondOpen.Entries);
        Assert.Equal("1800.00", secondOpen.Entries[0].Amount);
        Assert.Equal(rule.Id, secondOpen.Entries[0].RecurrenceRuleId);
    }

    [Fact]
    public async Task PausingRule_KeepsAlreadyMaterializedFutureOccurrence()
    {
        var options = CreateOptions();
        RecurrenceRuleResponse rule;
        await using (var db = new ValoraDbContext(options))
            rule = await new ValoraService(db, new FixedMonthClock(Current), new MonthSummaryCalculator()).CreateRuleAsync(
                new RecurrenceRequest("expense", "Rent", "2026-11", "copy_default", "1800.00"), CancellationToken.None);
        await using (var db = new ValoraDbContext(options))
            await new ValoraService(db, new FixedMonthClock(Current), new MonthSummaryCalculator()).OpenMonthAsync("2026-12", CancellationToken.None);

        await using (var db = new ValoraDbContext(options))
            await new ValoraService(db, new FixedMonthClock(Current), new MonthSummaryCalculator()).TransitionRuleAsync(rule.Id, "pause",
                new RecurrenceTransitionRequest("2026-12"), CancellationToken.None);
        MonthResponse december;
        await using (var db = new ValoraDbContext(options))
            december = await new ValoraService(db, new FixedMonthClock(Current), new MonthSummaryCalculator()).OpenMonthAsync("2026-12", CancellationToken.None);

        Assert.Single(december.Entries);
        Assert.Equal("1800.00", december.Entries[0].Amount);
    }

    [Fact]
    public async Task CreateEntry_RejectsChangesToPastMonths()
    {
        var options = CreateOptions();
        await using var db = new ValoraDbContext(options);
        var service = new ValoraService(db, new FixedMonthClock(Current), new MonthSummaryCalculator());

        var exception = await Assert.ThrowsAsync<ValoraException>(() => service.CreateEntryAsync("2026-10",
            new CreateEntryRequest("income", "Salary", "100.00", null), CancellationToken.None));

        Assert.Equal("month_read_only", exception.Code);
        Assert.Empty(db.Entries);
    }

    private static DbContextOptions<ValoraDbContext> CreateOptions()
    {
        var options = new DbContextOptionsBuilder<ValoraDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return options;
    }

    private sealed class FixedMonthClock(MonthKey currentMonth) : IMonthClock
    {
        public MonthKey CurrentMonth { get; } = currentMonth;
    }
}

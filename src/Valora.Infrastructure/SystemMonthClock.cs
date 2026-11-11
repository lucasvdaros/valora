using Microsoft.Extensions.Configuration;
using Valora.Application;
using Valora.Domain;

namespace Valora.Infrastructure;

public sealed class SystemMonthClock : IMonthClock
{
    private readonly TimeZoneInfo _timeZone;

    public SystemMonthClock(IConfiguration configuration)
    {
        var timeZoneId = configuration["Valora:TimeZone"] ?? "America/Sao_Paulo";
        _timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
    }

    public MonthKey CurrentMonth
    {
        get
        {
            var localNow = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, _timeZone);
            return new MonthKey(localNow.Year, localNow.Month);
        }
    }
}

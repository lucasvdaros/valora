using Microsoft.AspNetCore.Routing;
using Valora.Application;

namespace Valora.Api;

public static class ValoraApiEndpointExtensions
{
    public static IEndpointRouteBuilder MapValoraApiEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var api = endpoints.MapGroup("/api/v1");
        MapMonthEndpoints(api.MapGroup("/months"));
        MapRecurrenceEndpoints(api.MapGroup("/recurrence-rules"));
        return endpoints;
    }

    private static void MapMonthEndpoints(RouteGroupBuilder months)
    {
        months.MapGet("/{month}", (string month, IValoraService service, CancellationToken token) =>
            service.GetMonthAsync(month, oneOff: false, token)).Produces<MonthResponse>();
        months.MapGet("/{month}/one-off", (string month, IValoraService service, CancellationToken token) =>
            service.GetMonthAsync(month, oneOff: true, token)).Produces<MonthResponse>();
        months.MapPost("/{month}/open", (string month, IValoraService service, CancellationToken token) =>
            service.OpenMonthAsync(month, token)).Produces<MonthResponse>();
        months.MapPost("/{month}/entries", async (string month, CreateEntryRequest request, IValoraService service,
            CancellationToken token) => Results.Created($"/api/v1/months/{month}/entries",
                await service.CreateEntryAsync(month, request, token)))
            .Produces<EntryResponse>(StatusCodes.Status201Created);
        months.MapPatch("/{month}/entries/{entryId:guid}", (string month, Guid entryId,
            UpdateEntryRequest request, IValoraService service, CancellationToken token) =>
            service.UpdateEntryAsync(month, entryId, request, token)).Produces<EntryResponse>();
    }

    private static void MapRecurrenceEndpoints(RouteGroupBuilder rules)
    {
        rules.MapPost("", async (RecurrenceRequest request, IValoraService service, CancellationToken token) =>
            Results.Created("/api/v1/recurrence-rules", await service.CreateRuleAsync(request, token)))
            .Produces<RecurrenceRuleResponse>(StatusCodes.Status201Created);
        rules.MapPatch("/{ruleId:guid}", (Guid ruleId, RecurrenceRequest request, IValoraService service,
            CancellationToken token) => service.UpdateRuleAsync(ruleId, request, token)).Produces<RecurrenceRuleResponse>();
        rules.MapPost("/{ruleId:guid}/pause", (Guid ruleId, RecurrenceTransitionRequest request,
            IValoraService service, CancellationToken token) =>
            service.TransitionRuleAsync(ruleId, "pause", request, token)).Produces<RecurrenceRuleResponse>();
        rules.MapPost("/{ruleId:guid}/resume", (Guid ruleId, RecurrenceTransitionRequest request,
            IValoraService service, CancellationToken token) =>
            service.TransitionRuleAsync(ruleId, "resume", request, token)).Produces<RecurrenceRuleResponse>();
        rules.MapPost("/{ruleId:guid}/end", (Guid ruleId, RecurrenceTransitionRequest request,
            IValoraService service, CancellationToken token) =>
            service.TransitionRuleAsync(ruleId, "end", request, token)).Produces<RecurrenceRuleResponse>();
    }
}

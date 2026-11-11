using Valora.Api;
using Valora.Application;
using Valora.Domain;
using Valora.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddValoraApi()
    .AddValoraApplication()
    .AddValoraDomain()
    .AddValoraInfrastructure(builder.Configuration);

var app = builder.Build();
app.UseValoraApi();
app.Run();

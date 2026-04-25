using XRayne.Core;
using XRayne.Infrastructure;
using XRayne.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddXRayneCore();
builder.Services.AddXRayneInfrastructure(builder.Configuration);
builder.Services.AddXRayneRepositories(builder.Configuration);

var app = builder.Build();

app.MapControllers();

app.Run();

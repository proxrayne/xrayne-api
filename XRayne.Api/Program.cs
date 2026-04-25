using XRayne.Core;
using XRayne.Infrastructure;
using XRayne.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddCoreDependencies();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddRepositories(builder.Configuration);

var app = builder.Build();

app.MapControllers();

app.Run();

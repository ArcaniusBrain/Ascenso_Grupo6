using Ascenso.S4.Becas.Application.HistorialBecas;
using Ascenso.S4.Becas.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(ConsultarHistorialBecasHandler).Assembly));

builder.Services.AddS4Infrastructure(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapControllers();

app.Run();

public partial class Program;

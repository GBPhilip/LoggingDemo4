using System.Diagnostics;
using CarvedRock.Data;
using CarvedRock.Domain;
using Microsoft.AspNetCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);
//builder.Logging.AddFilter("CarvedRock", LogLevel.Debug);

// var path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
// var tracePath = Path.Join(path, $"Log_CarvedRock_{DateTime.Now.ToString("yyyyMMdd-HHmm")}.txt");        
// Trace.Listeners.Add(new TextWriterTraceListener(System.IO.File.CreateText(tracePath)));
// Trace.AutoFlush = true;	
builder.Services.AddProblemDetails(opts =>
opts.CustomizeProblemDetails=(ctx) =>
{
    var exception =ctx.HttpContext.Features.Get<IExceptionHandlerFeature>()?.Error;
    if (ctx.ProblemDetails.Status == 500)
    {
        ctx.ProblemDetails.Detail = "An error occurred in our API. Use the trace id when contacting us.";
    }
});
// Services
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IProductLogic, ProductLogic>();

builder.Services.AddDbContext<LocalContext>();
builder.Services.AddScoped<ICarvedRockRepository, CarvedRockRepository>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<LocalContext>();
    context.MigrateAndCreateData();
}
app.UseExceptionHandler();
// HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.OAuthClientId("interactive.public.short");
        options.OAuthAppName("CarvedRock API");
        options.OAuthUsePkce();
    });
}
app.MapFallback(() => Results.Redirect("/swagger"));
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

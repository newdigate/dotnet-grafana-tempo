using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);
builder
    .WebHost
    .UseUrls("http://*:1000", "https://*:1234", "http://0.0.0.0:5000");

builder.Logging
    .ClearProviders()
    .AddOpenTelemetry(options =>
    {
        options.IncludeFormattedMessage = true;
        options.IncludeScopes = true;
        var resBuilder = ResourceBuilder.CreateDefault();
        var serviceName = builder.Configuration["ServiceName"]!;
        resBuilder.AddService(serviceName);
        options.SetResourceBuilder(resBuilder);
        options.AddOtlpExporter();
    });

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddOpenTelemetry()
    .ConfigureResource(b =>
    {
        b.AddService(builder.Configuration["ServiceName"]!);
    })
    .WithTracing(b => b
        .AddAspNetCoreInstrumentation(o =>
        {
            o.Filter = ctx => ctx.Request.Path != "/metrics";
        })
        .AddHttpClientInstrumentation()
        .AddSource("TraceActivityName")
        .AddOtlpExporter( 
            c => { 
                c.Endpoint = new System.Uri("http://127.0.0.1:4317"); 
                c.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
            })
        .AddConsoleExporter())
    .WithMetrics(b => b
        .AddAspNetCoreInstrumentation(o =>
        {
            o.Filter = (_, ctx) => ctx.Request.Path != "/metrics";
        })
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        .AddProcessInstrumentation()
        .AddPrometheusExporter());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.UseOpenTelemetryPrometheusScrapingEndpoint();
app.UseHttpLogging();
app.Run();

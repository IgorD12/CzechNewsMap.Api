using CzechNewsMap.Api.Models;
using CzechNewsMap.Api.Services;

var builder = WebApplication.CreateBuilder(args);
var allowedOrigins = builder.Configuration.GetSection("Frontend:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddControllers();

builder.Services.AddSingleton<CzechLocationCatalog>();
builder.Services.AddSingleton<RssEventMapper>();
builder.Services.AddSingleton<CachedSourceEventService>();
builder.Services.AddScoped<ArticleDedupService>();
builder.Services.AddScoped<SourceDiagnosticsService>();

builder.Services.AddScoped<ISourceService, IdnesSourceService>();
builder.Services.AddHttpClient<ISourceService, NovinkySourceService>();
builder.Services.AddHttpClient<ISourceService, AktualneSourceService>();
builder.Services.AddHttpClient<ISourceService, IrozhlasSourceService>();
builder.Services.AddHttpClient<ISourceService, DenikSourceService>();
builder.Services.AddHttpClient<ISourceService, CeskeNovinySourceService>();
builder.Services.AddHttpClient<ISourceService, CnnPrimaSourceService>();
builder.Services.AddHttpClient<ISourceService, Ct24SourceService>();
builder.Services.AddHttpClient<ISourceService, SeznamZpravySourceService>();
// builder.Services.AddHttpClient<ISourceService, PoliciePrahaSourceService>(); // specialized source, keep disabled until parsing is stricter

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .AllowAnyHeader()
            .AllowAnyMethod();

        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins);
        }
        else if (builder.Environment.IsDevelopment())
        {
            policy.SetIsOriginAllowed(origin =>
                Uri.TryCreate(origin, UriKind.Absolute, out var uri) && uri.IsLoopback);
        }
        else
        {
            policy.SetIsOriginAllowed(_ => false);
        }
    });
});

builder.Services.Configure<RssOptions>(
    builder.Configuration.GetSection("Rss"));
builder.Services.Configure<SourceCacheOptions>(
    builder.Configuration.GetSection("SourceCache"));

builder.Services.AddHttpClient<RssService>();
builder.Services.AddSingleton<EventService>();

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("Frontend");

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new
{
    status = "ok",
    timestamp = DateTime.UtcNow
}));

app.MapControllers().RequireCors("Frontend");

app.Run();
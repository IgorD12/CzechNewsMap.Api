using CzechNewsMap.Api.Models;
using CzechNewsMap.Api.Services;
using CzechNewsMap.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient<PoliciePrahaSourceService>();
builder.Services.AddScoped<ISourceService, PoliciePrahaSourceService>();

builder.Services.AddControllers();
builder.Services.AddSingleton<RssEventMapper>();

builder.Services.AddScoped<ArticleDedupService>();
builder.Services.AddScoped<ISourceService, IdnesSourceService>();

builder.Services.AddSingleton<RssEventMapper>();
builder.Services.AddScoped<ArticleDedupService>();

builder.Services.AddScoped<ISourceService, IdnesSourceService>();
builder.Services.AddScoped<ISourceService, PoliciePrahaSourceService>();

builder.Services.AddHttpClient<RssService>();
builder.Services.AddHttpClient<PoliciePrahaSourceService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.Configure<RssOptions>(
    builder.Configuration.GetSection("Rss"));

builder.Services.AddHttpClient<RssService>();
builder.Services.AddSingleton<EventService>();

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("Frontend");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
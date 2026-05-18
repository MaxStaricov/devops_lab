using StackExchange.Redis;
using Microsoft.EntityFrameworkCore;
using Data;
using Models;
using Controllers;
using Services;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

var redisConnectionString = builder.Configuration.GetConnectionString("Redis");
IConnectionMultiplexer? redis = null;

if (!string.IsNullOrEmpty(redisConnectionString))
{
    try
    {
        redis = ConnectionMultiplexer.Connect(redisConnectionString);
        builder.Services.AddSingleton(redis);
        builder.Services.AddSingleton<ICacheService, RedisCacheService>();
    }
    catch
    {
        // Redis unavailable — fall back to no-op cache
    }
}

if (redis == null)
{
    builder.Services.AddSingleton<ICacheService, NullCacheService>();
}

var dbConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (!string.IsNullOrEmpty(dbConnectionString))
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(dbConnectionString));
}
else
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseInMemoryDatabase("TodoDb"));
}
builder.Services.AddControllers();

builder.Services.Configure<CacheSettings>(builder.Configuration.GetSection("CacheSettings"));

builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(); 

app.UseRouting();

app.UseHttpMetrics();

app.MapMetrics();

app.MapControllers();

if (redis != null)
{
    app.Lifetime.ApplicationStopping.Register(() =>
    {
        redis.Close();
    });
}

app.Run();
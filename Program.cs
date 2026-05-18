using StackExchange.Redis;
using Microsoft.EntityFrameworkCore;
using Data;
using Models;
using Controllers;
using Prometheus;

var builder = Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder(args);

var redis = ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis"));

builder.Services.AddSingleton<IConnectionMultiplexer>(redis);
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
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

app.Lifetime.ApplicationStopping.Register(() =>
{
    redis.Close();
});

app.Run();
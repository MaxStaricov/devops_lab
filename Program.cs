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

// Middleware для экспорта метрик Prometheus
app.UseRouting();

app.UseHttpMetrics(); // собирает базовые HTTP метрики

app.MapMetrics(); // создаёт endpoint /metrics


app.MapControllers();
app.Run();
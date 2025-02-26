using WeatherBot.Repositories;
using WeatherBot.Services;
using Microsoft.Data.SqlClient;
using System.Data;
using Microsoft.Extensions.DependencyInjection;
using WeatherBot.Extensions;


var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureServices(builder.Configuration);

var app = builder.Build();

await app.InitializeDatabaseAsync();

app.StartTelegramBot();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();

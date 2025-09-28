using Microsoft.OpenApi.Models;

const string url = "http://localhost:5220";
const string separator = "- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

var app = builder.Build();

// Allows us to view the endpoints at using Swagger
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Start();

Console.WriteLine(separator);
Console.WriteLine($"YOYOYO: Swagger is available at: {url}/swagger");
Console.WriteLine(separator);

app.WaitForShutdown();

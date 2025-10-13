using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using back.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
  c.SwaggerDoc("v1", new OpenApiInfo
  {
    Title = "komcon API",
    Version = "v1",
    Description = "komcon - Connect through music!"
  });
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
      options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
      options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register repositories for DI
builder.Services.AddScoped<back.Data.Repositories.ITeamsRepository, back.Data.Repositories.TeamsRepository>();
builder.Services.AddScoped<back.Data.Repositories.IChatsRepository, back.Data.Repositories.ChatsRepository>();
builder.Services.AddScoped<back.Data.Repositories.ISongsRepository, back.Data.Repositories.SongsRepository>();
builder.Services.AddScoped<back.Data.Repositories.IUsersRepository, back.Data.Repositories.UsersRepository>();

builder.Services.AddCors(options =>
{
  options.AddPolicy("AllowAll", builder =>
  {
    builder.AllowAnyOrigin()
             .AllowAnyMethod()
             .AllowAnyHeader();
  });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "komcon API"));
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
  var services = scope.ServiceProvider;
  try
  {
    var context = services.GetRequiredService<ApplicationDbContext>();
    context.Database.EnsureCreated();
    Console.WriteLine("Database created successfully.");
  }
  catch (Exception ex)
  {
    Console.WriteLine($"An error occurred while creating the database: {ex.Message}");
  }
}

app.Start();

const string separator = "- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -";

foreach (var url in app.Urls)
{
  Console.WriteLine($"{separator}\nkomcon API: Swagger is available at: {url}/swagger\n{separator}");
}

app.WaitForShutdown();

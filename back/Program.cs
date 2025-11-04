using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using back.Data;
using back.Services;
using back.Data.Repositories;

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
      // serialize enums as strings for readability on the client
      options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repository registrations (Scoped - one instance per HTTP request)
builder.Services.AddScoped<ITeamsRepository, TeamsRepository>();
builder.Services.AddScoped<IChatsRepository, ChatsRepository>();
builder.Services.AddScoped<ISongsRepository, SongsRepository>();
builder.Services.AddScoped<IUsersRepository, UsersRepository>();

// Service registrations
// Changed from Singleton to Scoped to avoid DI issues with repository dependencies
builder.Services.AddHttpClient();
builder.Services.AddHttpClient();
builder.Services.AddScoped<back.Services.ISongQueueService, back.Services.SongQueueService>();
builder.Services.AddScoped<back.Services.IDataImportService, back.Services.DataImportService>();
builder.Services.AddScoped<back.Validators.IYoutubeValidator, back.Validators.YoutubeValidator>();

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

  var importer = scope.ServiceProvider.GetRequiredService<IDataImportService>();
  await importer.ImportData("Dummy_data.json");
}

app.Start();

const string separator = "- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -";

foreach (var url in app.Urls)
{
  Console.WriteLine($"{separator}\nkomcon API: Swagger is available at: {url}/swagger\n{separator}");
}

app.WaitForShutdown();

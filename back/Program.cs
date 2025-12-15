using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using back.Data;
using back.Services;
using back.Data.Repositories;
using back.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddProvider(new FileLoggerProvider("Logs/log.txt"));


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
      options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddSignalR();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repository registrations
builder.Services.AddScoped<ITeamsRepository, TeamsRepository>();
builder.Services.AddScoped<IChatsRepository, ChatsRepository>();
builder.Services.AddScoped<ISongsRepository, SongsRepository>();
builder.Services.AddScoped<IUsersRepository, UsersRepository>();
builder.Services.AddScoped<IRatingsRepository, RatingsRepository>();
builder.Services.AddScoped<IGlobalUsersRepository, GlobalUsersRepository>();

// Utils
builder.Services.AddSingleton<back.Utils.IComparableUtils, back.Utils.ComparableUtils>();
builder.Services.AddSingleton<back.Cache.ISongQueuesCache, back.Cache.SongQueuesCache>();

// Service registrations
builder.Services.AddHttpClient();
builder.Services.AddScoped<back.Services.ISongQueueService, back.Services.SongQueueService>();
builder.Services.AddScoped<back.Services.IDataImportService, back.Services.DataImportService>();
builder.Services.AddScoped<back.Validators.IYoutubeValidator, back.Validators.YoutubeValidator>();
builder.Services.AddScoped<back.Services.IYoutubeDataService, back.Services.YoutubeDataService>();

builder.Services.AddCors(options =>
{
  options.AddPolicy("AllowAll", builder =>
  {
    builder.WithOrigins("http://localhost:5173", "https://localhost:5173")
           .AllowAnyMethod()
           .AllowAnyHeader()
           .AllowCredentials();
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
app.MapHub<TeamHub>("/teamHub");

using (var scope = app.Services.CreateScope())
{
  var services = scope.ServiceProvider;
  try
  {
    var context = services.GetRequiredService<ApplicationDbContext>();
    // Apply any pending migrations automatically
    context.Database.Migrate();
    Console.WriteLine("Migrations applied successfully.");
  }
  catch (Exception ex)
  {
    Console.WriteLine($"An error occurred while applying migrations: {ex.Message}");
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

public partial class Program { }

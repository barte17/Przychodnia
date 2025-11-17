using Microsoft.EntityFrameworkCore;
using Backend.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// CORS configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// PostgreSQL DbContext (Aiven)
builder.Services.AddDbContext<RelationalDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSQL")));

// MongoDB Context (Atlas)
builder.Services.AddSingleton<MongoDbContext>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthorization();

app.MapControllers();

// Initialize MongoDB indexes in background
_ = Task.Run(async () =>
{
    try
    {
        var mongoContext = app.Services.GetRequiredService<MongoDbContext>();
        await mongoContext.InitializeAsync();
        Console.WriteLine("MongoDB indexes initialized successfully");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error initializing MongoDB: {ex.Message}");
    }
});

app.Run();

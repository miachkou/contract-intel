using Infrastructure;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "Contract Intelligence API",
        Version = "v1",
        Description = "API for contract analysis, clause detection, and risk scoring"
    });
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Add Infrastructure services (DbContext with provider switch)
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Development-only: Ensure database is created and seeded
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ContractIntelDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    await dbContext.Database.EnsureCreatedAsync();
    await WebApi.DevDataSeeder.SeedAsync(dbContext, logger);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Contract Intelligence API v1");
        c.RoutePrefix = "swagger"; // Access at /swagger
    });
}

app.UseHttpsRedirection();

// Enable CORS (must be before MapControllers)
app.UseCors("AllowFrontend");

app.MapControllers();

app.Run();

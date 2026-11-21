using CourseAPI.Data;
using CourseAPI.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Kursplassen")
    ?? throw new InvalidOperationException("Connection string 'Kursplassen' mangler i konfigurasjonen.");

// Registrer password hasher som en tjeneste
builder.Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();

builder.Services.AddDbContext<KursplassenDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
           .UseSnakeCaseNamingConvention());

// Registrer tjenestene
builder.Services.AddOpenApi();

var app = builder.Build();

// Database: migrer og legg inn testdata (kun lokalt)
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<KursplassenDbContext>();
    await db.Database.MigrateAsync();
    await SeedData.SeedAsync(db);
}

// API-dokumentasjon (kun lokalt)
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();
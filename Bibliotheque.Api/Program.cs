using Bibliotheque.Core.Interfaces;
using Bibliotheque.Infrastructure.Data;
using Bibliotheque.Infrastructure.Repositories;
using Bibliotheque.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configuration de la base de données
builder.Services.AddDbContext<BibliothequeDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ADO.NET Context
builder.Services.AddScoped<AdoNetContext>();

// Repositories
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmpruntService, EmpruntService>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IStatistiquesService, StatistiquesService>();
builder.Services.AddScoped<IImportExportService, ImportExportService>();

// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Bibliotheque API", Version = "v1" });
});

// CORS pour le frontoffice
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontoffice", policy =>
    {
        policy.WithOrigins("https://localhost:5002", "http://localhost:5003")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontoffice");
app.UseAuthorization();
app.MapControllers();

app.Run();

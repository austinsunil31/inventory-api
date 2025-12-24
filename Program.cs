using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

//
// 🔹 Add services to the container
//

// Controllers (IMPORTANT)
builder.Services.AddControllers();

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database (Azure SQL)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

// CORS (allow Angular frontend)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

//
// 🔹 Configure the HTTP request pipeline
//

    app.UseSwagger();
    app.UseSwaggerUI();

app.UseHttpsRedirection();

// Enable CORS
app.UseCors("AllowAngular");

// (JWT will come here later)
// app.UseAuthentication();
app.UseAuthorization();

// Map controllers
app.MapControllers();

app.Run();

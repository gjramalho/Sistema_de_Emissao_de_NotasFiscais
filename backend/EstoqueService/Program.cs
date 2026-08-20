using EstoqueService.Data;
using EstoqueService.Middleware;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
   c.SwaggerDoc("v1", new() {Title = "Estoque Service", Version = "v1"}); 
});

builder.Services.AddDbContext<EstoqueDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("EstoqueDb")));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<EstoqueDbContext>();
    db.Database.EnsureCreated();
}

app.UseMiddleware<ExceptionMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAngular");
app.UseAuthorization();
app.MapControllers();

app.Run();

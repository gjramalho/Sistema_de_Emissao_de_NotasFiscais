using FaturamentoService.Data;
using FaturamentoService.Middleware;
using FaturamentoService.Services;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Extensions.Http;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Faturamento Service", Version = "v1" });
});

builder.Services.AddDbContext<FaturamentoDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("FaturamentoDb")));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});


var estoqueBaseUrl = builder.Configuration["EstoqueService:BaseUrl"] ?? "http://localhost:5001";

var retryPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
    .WaitAndRetryAsync(3, tentativa => TimeSpan.FromMilliseconds(200 * Math.Pow(2, tentativa - 1)));

var circuitBreakerPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .CircuitBreakerAsync(handledEventsAllowedBeforeBreaking: 3, durationOfBreak: TimeSpan.FromSeconds(15));

builder.Services.AddHttpClient<IEstoqueClient, EstoqueClient>(client =>
{
    client.BaseAddress = new Uri(estoqueBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(5);
})
.AddPolicyHandler(retryPolicy)
.AddPolicyHandler(circuitBreakerPolicy);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<FaturamentoDbContext>();
    db.Database.EnsureCreated();
}

app.UseMiddleware<ExceptionMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAngular");
app.UseAuthorization();
app.MapControllers();

app.Run();  

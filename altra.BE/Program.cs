using altra.BE;
using KiteConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddHttpClient<Groww>();
builder.Services.AddSingleton(sp => new Kite(TradingConstants.APIKEY));
builder.Services.AddSingleton<OrderManager>();
builder.Services.AddSingleton<LoginManager>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ✅ Allow frontend to call backend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy.AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod());
});

// ❌ Do NOT force localhost URLs in cloud
 // builder.WebHost.UseUrls("http://localhost:5000", "https://localhost:5001");

var app = builder.Build();

// ✅ Enable Swagger only in development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ❌ Remove HTTPS redirection in container
 // app.UseHttpsRedirection();

app.UseCors("AllowAll");   // must be before MapControllers
app.UseAuthorization();

app.MapControllers();

// ✅ Use PORT environment variable for Render
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
app.Urls.Add($"http://*:{port}");

app.Run();

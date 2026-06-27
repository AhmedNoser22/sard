using Sard.API.Filters;

using Sard.Infrastructure.Extensions;
//using Sard.Infrastructure.Seeders;
using System.Text.Json;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// ???? ???? ?????

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationFilter>();
})
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("https://sunna3.vercel.app")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});
builder.Services.AddSwaggerGen();

var app = builder.Build();

//using (var scope = app.Services.CreateScope())
//    await AdminSeeder.SeedAsync(scope.ServiceProvider);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}



app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseCors("AllowAngular");


app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();

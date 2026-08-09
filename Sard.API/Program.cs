var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationFilter>();
})
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.Converters.Add(
        new System.Text.Json.Serialization.JsonStringEnumConverter());
});
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        var origins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()
            ?? new[] { "http://localhost:4200" };

        policy.WithOrigins(origins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});
builder.Services.AddSingleton<IUserIdProvider, NameIdentifierUserIdProvider>();

builder.Services.AddSignalR();

builder.Services.AddSwaggerGen();

var app = builder.Build();

//using (var scope = app.Services.CreateScope())
//    await AdminSeeder.SeedAsync(scope.ServiceProvider);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.Use(async (context, next) =>
{
    context.Request.EnableBuffering();
    await next();
});
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("AllowAngular");
app.UseAuthentication();
app.UseAuthorization();

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAdminFilter() },
    DashboardTitle = "سرد — لوحة المهام",
});

app.MapControllers();
app.MapHub<NabdHub>("/hubs/nabd");
app.MapHub<GroupHub>("/hubs/group");

using (var scope = app.Services.CreateScope())
{
    RecurringJob.AddOrUpdate<TokenCleanupJob>(
        "cleanup-tokens",
        job => job.CleanExpiredTokensAsync(),
        Cron.Daily);

    RecurringJob.AddOrUpdate<StatsUpdateJob>(
        "update-novel-stats",
        job => job.UpdateNovelStatsAsync(),
        Cron.Hourly);
}

app.Run();

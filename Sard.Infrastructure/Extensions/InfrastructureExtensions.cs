using Sard.Application.Interfaces.Cache;
using Sard.Infrastructure.Implementation.Admin;
using Sard.Infrastructure.Implementation.Cache;
using Sard.Infrastructure.Implementation.Notification;
using Sard.Infrastructure.Implementation.Post;
using StackExchange.Redis;

namespace Sard.Infrastructure.Extensions
{
    public static class InfrastructureExtensions
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            //section for database context
            services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            //section for identity and authentication
            services.AddIdentity<AppUser, IdentityRole>(options =>
            {
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromDays(365 * 100);
                options.Lockout.MaxFailedAccessAttempts = 999;
            })
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                var jwt = configuration.GetSection("JwtSettings").Get<JwtSettings>()!;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwt.Key))
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            //section for caching
            services.AddMemoryCache();

            // section for redis caching
            var redisConnection = configuration.GetSection("RedisSettings:ConnectionString").Value!;
            services.AddSingleton<IConnectionMultiplexer>(
                ConnectionMultiplexer.Connect(redisConnection));

            //section for validation
            services.AddValidatorsFromAssembly(
                Assembly.Load("Sard.Application"),
                includeInternalTypes: true);

            // section for settings
            services.Configure<SmtpSettings>(configuration.GetSection("SmtpSettings"));
            services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
            services.Configure<GeminiSettings>(configuration.GetSection("GeminiSettings"));
            services.Configure<CloudinarySettings>(configuration.GetSection("CloudinarySettings"));
            services.Configure<RedisSettings>(configuration.GetSection("RedisSettings"));

            //section for services
            services.AddScoped<IImageService, ImageService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IGoogleAuthService, GoogleAuthService>();
            services.AddHttpClient<IAiService, AiService>();


            //section for application services
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IProfileService, ProfileService>();
            services.AddScoped<INovelService, NovelService>();
            services.AddScoped<IPostService, PostService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IAdminService, AdminService>();
            services.AddScoped<ICacheService, CacheService>();


            //real-time communication
            services.AddSignalR();

            // setting for payment service

            services.Configure<StripeSettings>(configuration.GetSection("StripeSettings"));
            services.AddScoped<IPaymentService, StripeService>();


            // section for pdf generation   
            services.AddScoped<NovelPdfService>();


            return services;
        }
    }
}

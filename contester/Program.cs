using System.Reflection;
using System.Text;
using Microsoft.EntityFrameworkCore;
using contester.Features.Authentication.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Sieve.Services;
using contester.Features.ApplicationSettings.Services;
using contester.Features.Attempts.Services;
using contester.Features.Contests.Services;
using contester.Features.Grade.Services;
using contester.Features.Scoreboard;
using FluentValidation;
using contester.Features.Scoreboard.Services;
using contester.Infrastructure;
using contester.Infrastructure.Persistence;
using contester.Infrastructure.Databases;
using contester.Infrastructure.MediatRBehaviors;
using contester.Infrastructure.Middleware;
using contester.Infrastructure.Seeders;
using contester.Infrastructure.Transpiler;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables(prefix: "Contester_");

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
{
    options.UseNpgsql(connectionString);
    options.AddInterceptors(sp.GetRequiredService<AuditableInterceptor>());
});
builder.Services.AddDbContext<OracleInitDbContext>(options =>
    options.UseOracle(builder.Configuration.GetConnectionString("OracleAdminConnection")), ServiceLifetime.Singleton);
builder.Services.AddDbContext<PostgresInitDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresAdminConnection")), ServiceLifetime.Singleton);
builder.Services.AddDbContext<SqlServerInitDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServerAdminConnection")), ServiceLifetime.Singleton);
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var configurationReader = new ConfigurationReaderService(builder.Configuration);

        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = configurationReader.GetJwtIssuer(),
            ValidAudience = $"{configurationReader.GetJwtIssuer()}/access",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configurationReader.GetJwtKey()))
        };
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        var configurationReader = new ConfigurationReaderService(builder.Configuration);

        policy.WithOrigins(configurationReader.GetFrontendUrl())
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddRazorPages();

builder.Services.AddScoped<SieveProcessor>();

builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<ISqlTranspilerService, SqlTranspilerService>();
builder.Services.AddScoped<IDirectoryService, DirectoryService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<ISolutionCheckerService, SolutionCheckerService>();
builder.Services.AddScoped<IGradeCalculationService, GradeCalculationService>();
builder.Services.AddScoped<IConfigurationReaderService, ConfigurationReaderService>();
builder.Services.AddScoped<IAuthorizationService, AuthorizationService>();
builder.Services.AddScoped<IContestService, ContestService>();
builder.Services.AddScoped<IAdminUserSeeder, AdminUserSeeder>();
builder.Services.AddScoped<ScoreboardUpdateNotifier>();
builder.Services.AddScoped<HealthCheckerService>();
builder.Services.AddScoped<AuditableInterceptor>();
builder.Services.AddScoped<IScoreboardService, ScoreboardService>();
builder.Services.AddScoped<ISolutionRunnerService, SolutionRunnerService>();
builder.Services.AddScoped<IAttemptExecutionContextFactory, AttemptExecutionContextFactory>();

builder.Services.AddSignalR();
builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());
builder.Services.AddMediatR(cfg =>
{
    var configurationReader = new ConfigurationReaderService(builder.Configuration);

    cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
    if (configurationReader.IsLoggingEnabled())
    {
        cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
    }
    cfg.AddOpenBehavior(typeof(AuthorizationBehavior<,>));
});
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

var app = builder.Build();

app.UseCors("CorsPolicy");


if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}
else
{
    app.UseMigrationsEndPoint();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.MapFallbackToFile("index.html");
app.MapHub<ScoreboardUpdatesHub>("/scoreboardUpdatesHub");


app.Services.GetService<SqlServerInitDbContext>()?.Init();
app.Services.GetService<PostgresInitDbContext>()?.Init();
app.Services.GetService<OracleInitDbContext>()?.Init();


using (var scope = app.Services.CreateScope())
{
    bool migrationsSucceeded = false;
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    for (int i = 0; i < 10; ++i)
        try
        {
            context.Database.Migrate();
            migrationsSucceeded = true;
            break;
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.Message);
            Console.Error.WriteLine($"Trying to reconnect in 10 seconds ({i}/10)");
            await Task.Delay(10_000);
        }

    if (!migrationsSucceeded)
        throw new ApplicationException("Could not connect to the database; please check PostgreSQL connection");

    var connectionStrings = context.ConnectionStrings.AsNoTracking().ToList();
    ConnectionStringsCache.Instance.SetCachedValues(connectionStrings);

    var adminUserSeeder = scope.ServiceProvider.GetRequiredService<IAdminUserSeeder>();
    await adminUserSeeder.Seed();
}

app.Run();

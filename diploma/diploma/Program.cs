using System.Reflection;
using System.Text;
using diploma.Application.Transpiler;
using Microsoft.EntityFrameworkCore;
using diploma.Data;
using diploma.Features.Authentication.Services;
using diploma.Middleware;
using diploma.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Sieve.Services;
using diploma.Data.Init;
using diploma.Features.Contests.Services;
using FluentValidation;
using diploma.Hubs;
using diploma.Features.Scoreboard.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables(prefix: "Contester_");

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));
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
        if (builder.Configuration["Jwt:Key"] is null) throw new Exception("Jwt:Key is not set");
        
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Issuer"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        if (builder.Configuration["App:FrontendUrl"] is null) throw new Exception("App:FrontendUrl is not set");
        policy.WithOrigins(builder.Configuration["App:FrontendUrl"]!)
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
builder.Services.AddScoped<ScoreboardUpdateNotifier>();

builder.Services.AddSignalR();
builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
    if (builder.Configuration["App:LoggingEnabled"] == "true") {
        cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
    }
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
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.Migrate();
}

app.Run();

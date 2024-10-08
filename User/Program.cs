using ArtHubDAO.DAO;
using ArtHubDAO.Data;
using ArtHubDAO.Interface;
using ArtHubRepository.DapperService;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text.Json.Serialization;
using ArtHubBO.Entities;
using ArtHubService.Service;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using User.Pages.Filter;
using User.Helpers;
using Microsoft.AspNetCore.DataProtection;
DotNetEnv.Env.Load();
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

//Add service dependency injection
builder.Services.Scan(scan => scan
    .FromAssembliesOf(new Type[] { typeof(DapperQueryService), typeof(AccountService)})
    .AddClasses(classes => classes.Where(type =>
     type.Name.EndsWith("Repository") || type.Name.EndsWith("Service")))
    .AsImplementedInterfaces()
    .WithScopedLifetime());
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});


//var redis = ConnectionMultiplexer
//    .Connect(Environment.GetEnvironmentVariable("REDIS_URL"));
//// Configure Redis Based Distributed Session
//var redisConfigurationOptions = builder.Configuration["REDIS_URL"];

//builder.Services.AddDataProtection()
//    .PersistKeysToStackExchangeRedis(redis, "Secrets-user-data-protection");
//builder.Services.AddStackExchangeRedisCache(redisCacheConfig =>
//{
//    redisCacheConfig.ConfigurationOptions = ConfigurationOptions.Parse(redisConfigurationOptions);
//});

builder.Services.AddSession(options => {
    options.Cookie.Name = "ArtworksSharingPlatform_Session";
    options.IdleTimeout = TimeSpan.FromMinutes(60 * 24);
});

// Logging configuration
builder.Host.ConfigureLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
});

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IBaseDAO<>), typeof(BaseDAO<>));
builder.Services.AddTransient<IHelper, Helper>();
string connectionString = builder.Configuration["DATABASE_URL"];
// Register IDbConnection in DI container
builder.Services.AddScoped<IDbConnection>((sp) => new SqlConnection(connectionString));
builder.Services.AddDbContext<ArtHubDbContext>(options =>
    options.UseSqlServer(builder.Configuration["DATABASE_URL"]));

builder.Services.AddMemoryCache();

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseStatusCodePages();
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseSession();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseMiddleware<LoginMiddleware>();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();

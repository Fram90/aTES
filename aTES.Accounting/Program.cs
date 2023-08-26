using System.Text;
using System.Text.Json.Serialization;
using aTES.Accounting.BillingCycleProcessing;
using aTES.Accounting.Db;
using aTES.Accounting.Domain.Services;
using aTES.Accounting.Kafka;
using aTES.Common.Shared.Db;
using Coravel;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var connectionString = builder.Configuration.GetConnectionString("AccountingConnection") ??
                       throw new InvalidOperationException("Connection string 'AuthConnection' not found.");

var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
var dataSource = dataSourceBuilder.Build();

builder.Services.AddDbContext<AccountingDbContext>(options =>
    options.UseNpgsql(dataSource));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// builder.Services.AddScoped<ValidationFilterAttribute>();
builder.Services.AddSwaggerGen();
builder.Services.AddMvc()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false)));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(o =>
{
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = false,
        ValidateIssuerSigningKey = true
    };
});
builder.Services.AddAuthorization();

// Add configuration from appsettings.json
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.AddScoped<AccountService>();
builder.Services.AddKafkaServices();
builder.Services.AddConsumers();
builder.Services.AddLogging();
builder.Services.AddSingleton<ILogger>(provider => provider.GetRequiredService<ILoggerFactory>().CreateLogger("basic"));

builder.Services.AddScheduler();

var app = builder.Build();

app.Services.UseScheduler(scheduler => scheduler.Schedule<CloseBillingCycleJob>().DailyAt(0, 0));
app.RunOutboxPublisher<AccountingDbContext>();

await DatabaseInitializer.Init<AccountingDbContext>(app);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSwagger();
app.UseSwaggerUI();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
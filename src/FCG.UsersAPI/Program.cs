using FCG.UsersAPI.Application.Abstractions.Messaging;
using FCG.UsersAPI.Extensions;
using FCG.UsersAPI.Extensions.ExtensionsLogs;
using FCG.UsersAPI.Infrastructure.Connections;
using FCG.UsersAPI.Messaging;
using Microsoft.EntityFrameworkCore;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();
builder.Services.AddApplicationServices(builder.Configuration);
//builder.Services.AddHttpContextAccessor();
//builder.Services.AddScoped<UserCreatedEventPublisher>();
//builder.Services.AddScoped<IUserCreatedEventPublisher,UserCreatedEventPublisher>();
builder.Services.AddScoped<IUserCreatedEventPublisher,UserCreatedEventPublisher>();
var rabbitMqHost =
    builder.Configuration["RabbitMq:Host"]
    ?? "localhost";

var rabbitMqUsername =
    builder.Configuration["RabbitMq:Username"]
    ?? "guest";

var rabbitMqPassword =
    builder.Configuration["RabbitMq:Password"]
    ?? "guest";
builder.Services.AddMassTransit(configuration =>
{
    configuration.UsingRabbitMq((context, rabbitMq) =>
    {
        //rabbitMq.Host("localhost", "/", host =>
        //{
        //    host.Username("guest");
        //    host.Password("guest");
        //});
        rabbitMq.Host("host.docker.internal", "/", host =>
        {
            //host.Username("guest");
            //host.Password("guest");
            host.Username(rabbitMqUsername);
            host.Password(rabbitMqPassword);
        });
    });
});

//builder.Services.AddScoped<IUserCreatedEventPublisher,UserCreatedEventPublisher>();

LogExtension.InitializeLogger();
var loggerSerialLog = LogExtension.GetLogger();
loggerSerialLog.Information("Logging initialized.");

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "FCG Users API v1");
    });
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseRouting();

app.UseCors("CorsPolicy");

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

var runMigrations = builder.Configuration.GetValue<bool>("RunMigrations");
if (runMigrations)
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;

    try
    {
        var context = services.GetRequiredService<DataContext>();
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occured during migration!");
    }
}

app.Run();
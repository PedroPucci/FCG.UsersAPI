//using FCG.UsersAPI.Application.Abstractions.Messaging;
//using FCG.UsersAPI.Extensions;
//using FCG.UsersAPI.Extensions.ExtensionsLogs;
//using FCG.UsersAPI.Infrastructure.Connections;
//using FCG.UsersAPI.Messaging;
//using Microsoft.EntityFrameworkCore;
//using MassTransit;

//var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddControllersWithViews();
//builder.Services.AddSession();
//builder.Services.AddHttpContextAccessor();
//builder.Services.AddApplicationServices(builder.Configuration);
//builder.Services.AddScoped<IUserCreatedEventPublisher,UserCreatedEventPublisher>();
//var rabbitMqHost =
//    builder.Configuration["RabbitMq:Host"]
//    ?? "localhost";

//var rabbitMqUsername =
//    builder.Configuration["RabbitMq:Username"]
//    ?? "guest";

//var rabbitMqPassword =
//    builder.Configuration["RabbitMq:Password"]
//    ?? "guest";
//builder.Services.AddMassTransit(configuration =>
//{
//    configuration.UsingRabbitMq((context, rabbitMq) =>
//    {
//        rabbitMq.Host("host.docker.internal", "/", host =>
//        {
//            host.Username(rabbitMqUsername);
//            host.Password(rabbitMqPassword);
//        });
//    });
//});

//LogExtension.InitializeLogger();
//var loggerSerialLog = LogExtension.GetLogger();
//loggerSerialLog.Information("Logging initialized.");

//var app = builder.Build();

//app.UseMiddleware<ExceptionMiddleware>();

//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI(c =>
//    {
//        c.SwaggerEndpoint("/swagger/v1/swagger.json", "FCG Users API v1");
//    });
//}

//if (!app.Environment.IsDevelopment())
//{
//    app.UseHttpsRedirection();
//}

//app.UseRouting();

//app.UseCors("CorsPolicy");

//app.UseSession();

//app.UseAuthentication();
//app.UseAuthorization();

//app.MapControllers();

//var runMigrations = builder.Configuration.GetValue<bool>("RunMigrations");
//if (runMigrations)
//{
//    using var scope = app.Services.CreateScope();
//    var services = scope.ServiceProvider;

//    try
//    {
//        var context = services.GetRequiredService<DataContext>();
//        context.Database.Migrate();
//    }
//    catch (Exception ex)
//    {
//        var logger = services.GetRequiredService<ILogger<Program>>();
//        logger.LogError(ex, "An error occured during migration!");
//    }
//}

//app.Run();

using FCG.UsersAPI.Application.Abstractions.Messaging;
using FCG.UsersAPI.Extensions;
using FCG.UsersAPI.Extensions.ExtensionsLogs;
using FCG.UsersAPI.Infrastructure.Connections;
using FCG.UsersAPI.Messaging;
using MassTransit;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// MVC, sessão e contexto HTTP
builder.Services.AddControllersWithViews();
builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();

// Serviços da aplicação
builder.Services.AddApplicationServices(builder.Configuration);

// Publicador de eventos
builder.Services.AddScoped<
    IUserCreatedEventPublisher,
    UserCreatedEventPublisher>();

// Configurações do RabbitMQ
var rabbitMqHost =
    builder.Configuration["RabbitMq:Host"]
    ?? "localhost";

var rabbitMqUsername =
    builder.Configuration["RabbitMq:Username"]
    ?? "guest";

var rabbitMqPassword =
    builder.Configuration["RabbitMq:Password"]
    ?? "guest";

var rabbitMqVirtualHost =
    builder.Configuration["RabbitMq:VirtualHost"]
    ?? "/";

// Configuração do MassTransit com RabbitMQ
builder.Services.AddMassTransit(configuration =>
{
    configuration.UsingRabbitMq((context, rabbitMq) =>
    {
        rabbitMq.Host(
            rabbitMqHost,
            rabbitMqVirtualHost,
            host =>
            {
                host.Username(rabbitMqUsername);
                host.Password(rabbitMqPassword);
            });

        // Cria automaticamente os endpoints configurados no projeto,
        // caso futuramente existam Consumers nesta API.
        rabbitMq.ConfigureEndpoints(context);
    });
});

// Inicialização do logger
LogExtension.InitializeLogger();

var loggerSerialLog = LogExtension.GetLogger();

loggerSerialLog.Information(
    "Logging initialized for FCG.UsersAPI.");

var app = builder.Build();

// Tratamento global de exceções
app.UseMiddleware<ExceptionMiddleware>();

// Swagger habilitado em Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "/swagger/v1/swagger.json",
            "FCG Users API v1");
    });
}

// Não usamos redirecionamento HTTPS dentro do container.
// O HTTPS pode ser tratado futuramente por um Ingress.
app.UseRouting();

app.UseCors("CorsPolicy");

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Executa migrations automaticamente quando configurado
var runMigrations =
    builder.Configuration.GetValue<bool>("RunMigrations");

if (runMigrations)
{
    await ApplyMigrationsAsync(app);
}

app.Run();

static async Task ApplyMigrationsAsync(WebApplication app)
{
    const int maximumAttempts = 10;
    var delayBetweenAttempts = TimeSpan.FromSeconds(5);

    using var scope = app.Services.CreateScope();

    var services = scope.ServiceProvider;

    var logger =
        services.GetRequiredService<ILogger<Program>>();

    var context =
        services.GetRequiredService<DataContext>();

    for (var attempt = 1; attempt <= maximumAttempts; attempt++)
    {
        try
        {
            logger.LogInformation(
                "Applying database migrations. Attempt {Attempt} of {MaximumAttempts}.",
                attempt,
                maximumAttempts);

            await context.Database.MigrateAsync();

            logger.LogInformation(
                "Database migrations applied successfully.");

            return;
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                exception,
                "Database migration attempt {Attempt} failed.",
                attempt);

            if (attempt == maximumAttempts)
            {
                logger.LogError(
                    exception,
                    "Database migrations failed after {MaximumAttempts} attempts.",
                    maximumAttempts);

                throw;
            }

            await Task.Delay(delayBetweenAttempts);
        }
    }
}
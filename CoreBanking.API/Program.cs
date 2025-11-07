
using CoreBanking.Core.Interfaces;
using CoreBanking.Infrastructure.Data;
using CoreBanking.Infrastructure.Repositories;
using CoreBanking.APP.Common.Behaviors;
using MediatR;
using Polly.Extensions.Http;
using Polly;
using Microsoft.EntityFrameworkCore;
using CoreBanking.APP.Accounts.Commands.CreateAccount;
using CoreBanking.APP.Common.Mappings;
using FluentValidation;
using CoreBanking.APP.Accounts.Commands.CreatedAccount;
using Microsoft.OpenApi.Models;
using CoreBanking.Core.Events;
using System.Reflection;
using CoreBanking.API.Mappings;
using CoreBanking.API.gRPC.Mappings;
using CoreBanking.APP.Customers.Commands.CreateCustomer;
using CoreBanking.APP.Common.Interfaces;
using CoreBanking.API.gRPC;
using CoreBanking.API.Middleware;
using CoreBanking.APP.Accounts.EventHandlers;
using CoreBanking.Infrastructure.Services;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using CoreBanking.API.Hubs.EventHandlers;
using CoreBanking.API.gRPC.Interceptors;
using CoreBanking.API.Hubs;
using CoreBanking.API.Hubs.Management;
using CoreBanking.API.Services;
using CoreBanking.Infrastructure.External.Resilience;
using CoreBanking.API.Extensions;


namespace CoreBanking.API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddDbContext<BankingDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


        builder.WebHost.ConfigureKestrel(options =>
        {
            // HTTP (for Swagger, REST, etc.)
            options.ListenLocalhost(5037, o =>
            {
                o.Protocols = HttpProtocols.Http1;
            });

            // HTTPS (for gRPC, requires HTTP/2)
            options.ListenLocalhost(7288, o =>
            {
                o.UseHttps(); // uses developer cert
                o.Protocols = HttpProtocols.Http2;
            });
        });
        // Register repositories
        builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
        builder.Services.AddScoped<IAccountRepository, AccountRepository>();
        builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();

        // Register UnitOfWork
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
        builder.Services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        builder.Services.AddTransient<INotificationHandler<AccountCreatedEvent>, AccountCreatedEventHandler>();
        builder.Services.AddTransient<INotificationHandler<MoneyTransferedEvent>, MoneyTransferedEventHandler>();
        builder.Services.AddTransient<INotificationHandler<InsufficientFundEvent>, InsufficientFundsEventHandler>();
        builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(DomainEventsBehavior<,>));
        builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehaviour<,>));

        // builder.Services.AddAutoMapper(cfg => { }, typeof(AccountProfile).Assembly);


        // Register AutoMapper to scan both APP and API assemblies for profiles so we can be able to map the request to the command and the entity to the command

        builder.Services.AddAutoMapper(cfg => { },
            typeof(AccountProfile).Assembly,
            typeof(RequestToCommandProfile).Assembly); // API layer mappings (Request -> Command)


        // Register MediatR with handlers and behaviors
        // builder.Services.AddMediatR(cfg =>
        // {
        //     // Note: Registering one command is enough per Layer—MediatR scans the entire Application assembly (all Commands & Queries).
        //     cfg.RegisterServicesFromAssembly(typeof(CreateAccountCommand).Assembly);

        //     // Register behaviors (order matters: first registered = outermost)
        //     cfg.AddOpenBehavior(typeof(LoggingBehaviour<,>));      // Logs everything (outermost)
        //     cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));    // Validates (inside logging)

        //     // Set service lifetime
        //     cfg.Lifetime = ServiceLifetime.Scoped;
        // });


        builder.Services.AddGrpc(options =>
        {
            options.EnableDetailedErrors = true;
            options.Interceptors.Add<ExceptionInterceptor>();
            options.MaxReceiveMessageSize = 16 * 1024 * 1024; // 16MB
            options.MaxSendMessageSize = 16 * 1024 * 1024; // 16MB
        });
        builder.Services.AddGrpcReflection();

        // Add SignalR
        builder.Services.AddSignalR(options =>
        {
            options.EnableDetailedErrors = builder.Environment.IsDevelopment();
            options.KeepAliveInterval = TimeSpan.FromSeconds(15);
            options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
            options.MaximumReceiveMessageSize = 64 * 1024; // 64KB
        })
        .AddMessagePackProtocol(); // For smaller message sizes

        // Register hub filters
        builder.Services.AddSingleton<ErrorHandlingHubFilter>();

        builder.Services.AddSingleton<ConnectionStateService>();

        builder.Services.AddHostedService<TransactionBroadcastService>();

        // Register external HTTP clients
        builder.Services.AddExternalHttpClients(builder.Configuration);

        // Add resilience services
        builder.Services.AddSingleton<IResilientHttpClientService, ResilientHttpClientService>();

        // Register Polly policies
        builder.Services.AddSingleton(HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => !msg.IsSuccessStatusCode)
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    var logger = CoreBanking.Infrastructure.External.Resilience.ContextExtensions.GetLogger(context);
                    logger?.LogWarning("Retry {RetryCount} after {Delay}ms",
                        retryCount, timespan.TotalMilliseconds);
                }));

        // Add MediatR with handlers and behaviors
        builder.Services.AddMediatR(cfg =>
       {
           cfg.RegisterServicesFromAssembly(typeof(CreateAccountCommand).Assembly);

           cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
           cfg.AddOpenBehavior(typeof(LoggingBehaviour<,>));
           cfg.AddOpenBehavior(typeof(DomainEventsBehavior<,>));

           cfg.Lifetime = ServiceLifetime.Scoped;
       });

        // Add Validators and autoMapper

        builder.Services.AddValidatorsFromAssembly(typeof(CreateAccountCommandValidator).Assembly);
        builder.Services.AddAutoMapper(cfg => { }, typeof(AccountProfile).Assembly);
        builder.Services.AddAutoMapper(cfg => { }, typeof(AccountGrpcProfile).Assembly);

        // Register outbox services
        builder.Services.AddScoped<IOutboxMessageProcessor, OutboxMessageProcessor>();
        builder.Services.AddHostedService<OutboxBackgroundService>();
        builder.Services.AddScoped<INotificationHandler<MoneyTransferedEvent>, RealTimeNotificationEventHandler>();


        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();

        // Add Swagger/OpenAPI
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "CoreBanking API",
                Version = "v1",
                Description = "A modern banking API built with Clean Architecture and CQRS",
                Contact = new OpenApiContact
                {
                    Name = "CoreBanking Team",
                    Email = "support@corebanking.com"
                }
            });

            // Include XML comments
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            c.IncludeXmlComments(xmlPath);

            // Add authentication support in Swagger
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme.",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });
        });
        var app = builder.Build();

        // app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
        // app.UseSwagger(options => options.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi2_0);
        // app.UseSwaggerUI(c =>
        // {
        //     c.SwaggerEndpoint("/swagger/v1/swagger.json", "CoreBanking API v1");
        //     c.RoutePrefix = "swagger"; // Access at /swagger
        //     c.DocumentTitle = "CoreBanking API Documentation";
        //     c.EnableDeepLinking();
        //     c.DisplayOperationId();
        // });



        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger(options => options.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi2_0);

            // Enriched Swagger UI
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "CoreBanking API v1");
                c.RoutePrefix = "swagger"; // Access at /swagger
                c.DocumentTitle = "CoreBanking API Documentation";
                c.EnableDeepLinking();
                c.DisplayOperationId();
            });
        }

        app.UseHttpsRedirection();

        app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

        app.UseAuthorization();

        app.MapControllers();

        app.MapHub<NotificationHub>("/hubs/notifications");
        app.MapHub<TransactionHub>("/hubs/transactions");
        app.MapHub<EnhancedNotificationHub>("/hubs/enhanced-notifications");

        // Configure gRPC services

        app.MapGrpcService<AccountGrpcService>();
        app.MapGrpcService<EnhancedAccountGrpcService>();
        //app.MapGrpcService<TradingGrpcService>();

        //Use grpc Endpoints
        app.MapGet("/", () => "CoreBanking API is running. Use /swagger for REST or a gRPC client for gRPC calls.");
        if (app.Environment.IsDevelopment())
        {
            app.MapGrpcReflectionService();
        }

        app.Run();

    }
}



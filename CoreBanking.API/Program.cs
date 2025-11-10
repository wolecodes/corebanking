
using CoreBanking.API.Extensions;
using CoreBanking.API.gRPC;
using CoreBanking.API.gRPC.Interceptors;
using CoreBanking.API.gRPC.Mappings;
using CoreBanking.API.Hubs;
using CoreBanking.API.Hubs.EventHandlers;
using CoreBanking.API.Hubs.Management;
using CoreBanking.API.Mappings;
using CoreBanking.API.Middleware;
using CoreBanking.API.Services;
using CoreBanking.APP.Accounts.Commands.CreateAccount;
using CoreBanking.APP.Accounts.Commands.CreatedAccount;
using CoreBanking.APP.Accounts.EventHandlers;
using CoreBanking.APP.Common.Behaviors;
using CoreBanking.APP.Common.Interfaces;
using CoreBanking.APP.Common.Mappings;
using CoreBanking.APP.Customers.Commands.CreateCustomer;
using CoreBanking.APP.External.HttpClients;
using CoreBanking.APP.External.Interfaces;
using CoreBanking.Core.Events;
using CoreBanking.Core.Interfaces;
using CoreBanking.Infrastructure.Data;
using CoreBanking.Infrastructure.External.Resilience;
using CoreBanking.Infrastructure.Repositories;
using CoreBanking.Infrastructure.Services;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Polly;
using Polly.Extensions.Http;


namespace CoreBanking.API;

public class Program
{ 
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // ------------------- SERVICES -------------------

        builder.Services.AddDbContext<BankingDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

        // Core dependencies
        builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
        builder.Services.AddScoped<IAccountRepository, AccountRepository>();
        builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
        builder.Services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        // Event handlers
        builder.Services.AddTransient<INotificationHandler<AccountCreatedEvent>, AccountCreatedEventHandler>();
        builder.Services.AddTransient<INotificationHandler<MoneyTransferedEvent>, MoneyTransferedEventHandler>();
        builder.Services.AddTransient<INotificationHandler<InsufficientFundEvent>, InsufficientFundsEventHandler>();
        builder.Services.AddTransient<INotificationHandler<MoneyTransferedEvent>, RealTimeNotificationEventHandler>();

        // Pipeline behaviors
        builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(DomainEventsBehavior<,>));
        builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehaviour<,>));

        // gRPC + Reflection
        builder.Services.AddGrpc(options =>
        {
            options.EnableDetailedErrors = true;
        });
        builder.Services.AddGrpcReflection();

        // SignalR
        builder.Services.AddSignalR();

        // MediatR setup
        builder.Services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(CreateAccountCommand).Assembly);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            cfg.AddOpenBehavior(typeof(LoggingBehaviour<,>));
            cfg.AddOpenBehavior(typeof(DomainEventsBehavior<,>));
        });

        // Validation and mapping
        builder.Services.AddValidatorsFromAssembly(typeof(CreateAccountCommandValidator).Assembly);
        builder.Services.AddAutoMapper(cfg => { }, typeof(AccountProfile).Assembly);
        builder.Services.AddAutoMapper(cfg => { }, typeof(AccountGrpcProfile).Assembly);

        // Outbox
        builder.Services.AddScoped<IOutboxMessageProcessor, OutboxMessageProcessor>();
        builder.Services.AddHostedService<OutboxBackgroundService>();

        // Controllers + Swagger
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "CoreBanking API",
                Version = "v1",
                Description = "A modern banking API built with Clean Architecture, DDD, and CQRS"
            });
        });

        // Kestrel multi-protocol setup
        builder.WebHost.ConfigureKestrel(options =>
        {
            // HTTP/1.1 for REST, Swagger, etc.
            options.ListenLocalhost(5037, o => o.Protocols = HttpProtocols.Http1);

            // HTTP/2 for gRPC
            options.ListenLocalhost(7288, o =>
            {
                o.UseHttps();
                o.Protocols = HttpProtocols.Http2;
            });
        });
        // Add SignalR services
        builder.Services.AddSignalR(options =>
        {
            options.EnableDetailedErrors = builder.Environment.IsDevelopment();
            options.KeepAliveInterval = TimeSpan.FromSeconds(15);
            options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
            options.MaximumReceiveMessageSize = 64 * 1024; // 64KB
        })
        .AddMessagePackProtocol();

        // Add connection state management
        builder.Services.AddSingleton<ConnectionStateService>();

        // Add hosted services
        builder.Services.AddHostedService<TransactionBroadcastService>();

        // Add external HTTP clients with resilience
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
                    var logger = ContextExtensions.GetLogger(context);
                    logger?.LogWarning("Retry {RetryCount} after {Delay}ms",
                        retryCount, timespan.TotalMilliseconds);
                }));

        builder.Services.AddHttpClient<ICreditScoringServiceClient, CreditScoringServiceClient>(client =>
        {
            client.BaseAddress = new Uri(builder.Configuration["CreditScoringApi:BaseUrl"] ?? "https://api.example.com");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });


        var app = builder.Build();

        // ------------------- PIPELINE -------------------

        app.UseHttpsRedirection();

        app.UseStaticFiles(); // Enables wwwroot

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger(options => options.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi2_0);
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "CoreBanking API v1");
                c.RoutePrefix = "swagger";
            });

            app.MapGrpcReflectionService();
        }

        app.UseAuthorization();

        app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

        // ------------------- ROUTING -------------------

        // REST API
        app.MapControllers();

        // gRPC endpoints
        app.MapGrpcService<AccountGrpcService>();
        app.MapGrpcService<EnhancedAccountGrpcService>();

        // SignalR hub
        app.MapHub<EnhancedNotificationHub>("/hubs/enhanced-notifications");
        app.MapHub<NotificationHub>("/hubs/notifications");
        app.MapHub<TransactionHub>("/hubs/transactions");

        // Static file fallback (optional)
        app.MapFallbackToFile("index.html");

        // Root landing page
        app.MapGet("/", () => "CoreBanking API is running. Visit /swagger for REST or use gRPC client.");

        app.Run();
    }
}



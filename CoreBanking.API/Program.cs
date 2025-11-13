using CoreBanking.API.gRPC.Mappings;
using CoreBanking.API.Hubs;
using CoreBanking.API.Hubs.EventHandlers;
using CoreBanking.API.Hubs.Management;
using CoreBanking.API.Middleware;
using CoreBanking.API.gRPC;
using CoreBanking.API.Services;
using CoreBanking.APP.Accounts.Commands.CreatedAccount;
using CoreBanking.APP.Accounts.Commands.CreateAccount;
using CoreBanking.APP.Accounts.EventHandlers;
using CoreBanking.APP.Common.Behaviors;
using CoreBanking.APP.Common.Interfaces;
using CoreBanking.APP.Common.Mappings;
using CoreBanking.APP.Common.Models;
using CoreBanking.APP.External.HttpClients;
using CoreBanking.APP.External.Interfaces;
using CoreBanking.Application.Transactions.EventHandlers;
using CoreBanking.Core.Events;
using CoreBanking.Core.Interfaces;
using CoreBanking.Infrastructure.Data;
using CoreBanking.Infrastructure.External.Resilience;
using CoreBanking.Infrastructure.Repositories;
using CoreBanking.Infrastructure.ServiceBus;
using CoreBanking.Infrastructure.ServiceBus.Handlers;
using CoreBanking.Infrastructure.Services;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Polly;
using Polly.Extensions.Http;
using CoreBanking.Application.BackgroundJobs;
using CoreBanking.APP.BackgroundJobs;
using Hangfire;
using CoreBanking.Infrastructure.BackgroundJobs;
using CoreBanking.API.Extensions;
using Hangfire.Storage;

namespace CoreBanking.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // =====================================================================
            // DATABASE
            // =====================================================================
            builder.Services.AddDbContext<BankingDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // =====================================================================
            // CORE & INFRASTRUCTURE
            // =====================================================================
            builder.Services.AddScoped<IAccountRepository, AccountRepository>();
            builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
            builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
            builder.Services.AddSingleton<IEventPublisher, ServiceBusEventPublisher>();
            builder.Services.AddScoped<IMonitoringApi>(provider =>
            {
               
        
                return JobStorage.Current.GetMonitoringApi();
            });

            // =====================================================================
            // EXTERNAL SERVICES + RESILIENCE
            // =====================================================================
            builder.Services.AddHttpClient<ICreditScoringServiceClient, CreditScoringServiceClient>(client =>
            {
                client.BaseAddress = new Uri(builder.Configuration["CreditScoringApi:BaseUrl"] ?? "https://api.example.com");
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            });

            builder.Services.AddSingleton<ISimulatedCreditScoringService, SimulatedCreditScoringService>();
            builder.Services.AddSingleton<IResilientHttpClientService, ResilientHttpClientService>();
            builder.Services.AddScoped<IResilienceService, ResilienceService>();
            builder.Services.Configure<ResilenceOptions>(builder.Configuration.GetSection("Resilience"));

            // =====================================================================
            // AZURE SERVICE BUS (MOCK-SAFE CONFIG)
            // =====================================================================
            builder.Services.Configure<ServiceBusConfiguration>(builder.Configuration.GetSection("ServiceBus"));
            builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<ServiceBusConfiguration>>().Value);

            builder.Services.AddSingleton<IServiceBusClientFactory>(provider =>
            {
                var env = provider.GetRequiredService<IHostEnvironment>();
                var logger = provider.GetRequiredService<ILogger<ServiceBusClientFactory>>();
                var config = provider.GetRequiredService<IOptions<ServiceBusConfiguration>>().Value;

                var connectionString = env.IsDevelopment() || string.IsNullOrWhiteSpace(config.ConnectionString)
                    ? "Endpoint=sb://mock-servicebus/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=mock"
                    : config.ConnectionString;

                return new ServiceBusClientFactory(connectionString, logger);
            });

            builder.Services.AddSingleton<ServiceBusAdministration>(provider =>
            {
                var env = provider.GetRequiredService<IHostEnvironment>();
                var logger = provider.GetRequiredService<ILogger<ServiceBusAdministration>>();
                var config = provider.GetRequiredService<IOptions<ServiceBusConfiguration>>().Value;

                var connectionString = env.IsDevelopment() || string.IsNullOrWhiteSpace(config.ConnectionString)
                    ? "Endpoint=sb://mock-servicebus/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=mock"
                    : config.ConnectionString;

                return new ServiceBusAdministration(connectionString, config, logger);
            });

            builder.Services.AddSingleton<IBankingServiceBusSender>(provider =>
            {
                var logger = provider.GetRequiredService<ILogger<BankingServiceBusSender>>();
                var configuration = provider.GetRequiredService<IConfiguration>();

                var conn = configuration.GetConnectionString("ServiceBus")
                    ?? "Endpoint=sb://mock-servicebus/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=mock";

                return new BankingServiceBusSender(conn, logger);
            });

            builder.Services.AddSingleton<IDeadLetterQueueProcessor, DeadLetterQueueProcessor>();
            builder.Services.AddSingleton<CustomerEventServiceBusHandler>();
            builder.Services.AddSingleton<TransactionEventServiceBusHandler>();

            builder.Services.AddHostedService<MessageProcessingService>();
            builder.Services.AddHostedService<DeadLetterQueueMonitorService>();

            // Fraud detection (mock-safe)
            builder.Services.AddScoped<IFraudDetectionService, MockFraudDetectionService>();

            // =====================================================================
            // DOMAIN EVENT HANDLERS
            // =====================================================================
            builder.Services.AddTransient<INotificationHandler<AccountCreatedEvent>, AccountCreatedEventHandler>();
            builder.Services.AddTransient<INotificationHandler<MoneyTransferedEvent>, MoneyTransferredEventHandler>();
            builder.Services.AddTransient<INotificationHandler<InsufficientFundEvent>, InsufficientFundsEventHandler>();
            builder.Services.AddTransient<INotificationHandler<MoneyTransferedEvent>, RealTimeNotificationEventHandler>();

            // =====================================================================
            // PIPELINE BEHAVIORS (MEDIATR)
            // =====================================================================
            builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(DomainEventsBehavior<,>));
            builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehaviour<,>));

            // =====================================================================
            // gRPC
            // =====================================================================
            builder.Services.AddGrpc(options => options.EnableDetailedErrors = true);
            builder.Services.AddGrpcReflection();

            // =====================================================================
            // SIGNALR
            // =====================================================================
            builder.Services.AddSignalR(options =>
            {
                options.EnableDetailedErrors = builder.Environment.IsDevelopment();
                options.KeepAliveInterval = TimeSpan.FromSeconds(15);
                options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
                options.MaximumReceiveMessageSize = 64 * 1024; // 64KB
            }).AddMessagePackProtocol();

            builder.Services.AddSingleton<ConnectionStateService>();
            // builder.Services.AddHostedService<TransactionBroadcastService>(); // TODO: Implement TransactionBroadcastService
            builder.Services.AddScoped<INotificationBroadcaster, NotificationBroadcaster>();

            // =====================================================================
            // POLLY
            // =====================================================================
            builder.Services.AddSingleton(HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => !msg.IsSuccessStatusCode)
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (outcome, timespan, retryCount, context) =>
                    {
                        var logger = CoreBanking.API.Extensions.PollyContextExtensions.GetLogger(context);
                        logger?.LogWarning("Retry {RetryCount} after {Delay}ms", retryCount, timespan.TotalMilliseconds);
                    }));

            builder.Services.AddSingleton<AdvancedPollyPolicies>();

            // =====================================================================
            // VALIDATION, MAPPING & OUTBOX
            // =====================================================================
            builder.Services.AddValidatorsFromAssembly(typeof(CreateAccountCommandValidator).Assembly);
            builder.Services.AddAutoMapper(cfg => { }, typeof(AccountProfile).Assembly);
            builder.Services.AddAutoMapper(cfg => { }, typeof(AccountGrpcProfile).Assembly);

            builder.Services.AddScoped<IOutboxMessageProcessor, OutboxMessageProcessor>();
            builder.Services.AddHostedService<OutboxBackgroundService>();

            // =====================================================================
            // MEDIATR CONFIGURATION
            // =====================================================================
            builder.Services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(CreateAccountCommand).Assembly);
                cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
                cfg.AddOpenBehavior(typeof(LoggingBehaviour<,>));
                cfg.AddOpenBehavior(typeof(DomainEventsBehavior<,>));
            });

            // Hangfire Configuration
            builder.Services.Configure<HangfireConfiguration>(builder.Configuration.GetSection("Hangfire"));
            builder.Services.AddHangfireServices(builder.Configuration);

            builder.Services.AddSingleton<LogJobFilter>();
            builder.Services.AddHangfire(config => config
                .UseSqlServerStorage(builder.Configuration.GetConnectionString("HangfireConnection")));
            builder.Services.AddHangfireServer();

            // Background Job Services
            builder.Services.AddScoped<IDailyStatementService, DailyStatementService>();
            builder.Services.AddScoped<IInterestCalculationService, InterestCalculationService>();
            builder.Services.AddScoped<IAccountMaintenanceService, AccountMaintenanceService>();
            builder.Services.AddScoped<IFailedJobHandler, FailedJobHandler>();
            builder.Services.AddScoped<IJobInitializationService, JobInitializationService>();
            builder.Services.AddScoped<IJobMonitoringService, JobMonitoringService>();

            // =====================================================================
            // CONTROLLERS + SWAGGER
            // =====================================================================
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

            // =====================================================================
            // KESTREL (HTTP/1.1 + HTTP/2)
            // =====================================================================
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ListenLocalhost(5037, o => o.Protocols = HttpProtocols.Http1);
                options.ListenLocalhost(7288, o =>
                {
                    o.UseHttps();
                    o.Protocols = HttpProtocols.Http2;
                });
            });

            // =====================================================================
            // APP PIPELINE
            // =====================================================================
            var app = builder.Build();

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseAuthorization();
            app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

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
            var logger = app.Services.GetRequiredService<ILogger<Program>>();
            using (var scope = app.Services.CreateScope())
            {
                var filter = scope.ServiceProvider.GetRequiredService<LogJobFilter>();
                GlobalJobFilters.Filters.Add(filter);

                logger.LogInformation("LogJobFilter registered globally");
            }
            // Hangfire Dashboard (secured)
            app.UseHangfireDashboardWithAuth();

            // Initialize jobs on startup
            using (var scope = app.Services.CreateScope())
            {
                var jobInitialization = scope.ServiceProvider.GetRequiredService<IJobInitializationService>();
                await jobInitialization.InitializeRecurringJobsAsync();
                await jobInitialization.RegisterOneTimeJobsAsync();
            }

            // Ensure mock-safe Service Bus setup
            using (var scope = app.Services.CreateScope())
            {
                var logger1 = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                var admin = scope.ServiceProvider.GetRequiredService<ServiceBusAdministration>();

                try
                {
                    await admin.EnsureInfrastructureExistsAsync();
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "[Startup] Skipping Service Bus setup (mock or offline).");
                }
            }

            // =====================================================================
            // ROUTING
            // =====================================================================
            app.MapControllers();
            app.MapGrpcService<AccountGrpcService>();
            app.MapGrpcService<EnhancedAccountGrpcService>();

            app.MapHub<NotificationHub>("/hubs/notifications");
            app.MapHub<EnhancedNotificationHub>("/hubs/enhanced-notifications");
            app.MapHub<TransactionHub>("/hubs/transactions");

            app.MapFallbackToFile("index.html");
            app.MapGet("/", () => "CoreBanking API is running. Visit /swagger for REST or use a gRPC client.");

            app.Run();
        }
    }
}
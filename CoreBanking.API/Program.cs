
using CoreBanking.Core.Interfaces;
using CoreBanking.Infrastructure.Data;
using CoreBanking.Infrastructure.Repositories;
using CoreBanking.APP.Common.Behaviors;
using MediatR;
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
using CoreBanking.gRPC.Services;
using CoreBanking.API.Middleware; 
using CoreBanking.APP.Accounts.EventHandlers;
using CoreBanking.Infrastructure.Services;
using Microsoft.AspNetCore.Server.Kestrel.Core;


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
            options.ListenLocalhost(5037, o => { o.Protocols = HttpProtocols.Http1; });

            options.ListenLocalhost(5038, o => { o.Protocols = HttpProtocols.Http2; });
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
        });
        builder.Services.AddGrpcReflection();

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

        //Use grpc Endpoints
        app.MapGrpcService<AccountGrpcService>();
        app.MapGet("/", () => "CoreBanking API is running. Use /swagger for REST or a gRPC client for gRPC calls.");
        if (app.Environment.IsDevelopment())
        {
            app.MapGrpcReflectionService();
        }

        app.Run();

    }
}



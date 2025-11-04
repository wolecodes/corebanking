
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
using System.Reflection;
using CoreBanking.API.Middleware;
using CoreBanking.API.Mappings;
using CoreBanking.APP.Customers.Commands.CreateCustomer;


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

        // Register repositories
        builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
        builder.Services.AddScoped<IAccountRepository, AccountRepository>();
        builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();

        // Register UnitOfWork
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

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



        builder.Services.AddValidatorsFromAssembly(typeof(CreateAccountCommandValidator).Assembly);
        builder.Services.AddValidatorsFromAssembly(typeof(CreateCustomerValidator).Assembly);

        // Add MediatR with behaviors
        builder.Services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(CreateCustomerCommand).Assembly);
            cfg.RegisterServicesFromAssembly(typeof(CreateAccountCommand).Assembly);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            cfg.AddOpenBehavior(typeof(LoggingBehaviour<,>));

            cfg.Lifetime = ServiceLifetime.Scoped;
        });
        // builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        // builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehaviour<,>));

        // Add Swagger/OpenAPI
        builder.Services.AddEndpointsApiExplorer();
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

        app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
        app.UseSwagger(options => options.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi2_0);
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "CoreBanking API v1");
            c.RoutePrefix = "swagger"; // Access at /swagger
            c.DocumentTitle = "CoreBanking API Documentation";
            c.EnableDeepLinking();
            c.DisplayOperationId();
        });

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();


        app.MapControllers();

        app.Run();
    }
}



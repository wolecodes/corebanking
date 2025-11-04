
using CoreBanking.Core.Entities;
using CoreBanking.Core.Enums;
using CoreBanking.Core.ValueObjects;
using Microsoft.EntityFrameworkCore;
using CoreBanking.Core.Common;
using CoreBanking.Infrastructure.Persistence.Outbox;
using System.Text.Json;

namespace CoreBanking.Infrastructure.Data;

public class BankingDbContext : DbContext
{
    public BankingDbContext(DbContextOptions<BankingDbContext> options) : base(options) { }
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<OutboxMessage> OutboxMessages { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());


        // softdelete filter
        modelBuilder.Entity<Customer>().HasQueryFilter(c => !c.IsDeleted);
        modelBuilder.Entity<Account>().HasQueryFilter(a => !a.IsDeleted);


        // Seed the DB
        modelBuilder.Entity<Customer>().HasData(new
        {
            CustomerId = CustomerId.Create(Guid.Parse("a1b2c3d4-1234-5678-9abc-123456789abc")),
            FirstName = "Alice",
            LastName = "Johnson",
            Email = "alice.johnson@email.com",
            PhoneNumber = "555-0101",
            DateCreated = DateTime.UtcNow.AddDays(-30),
            IsActive = true,
            IsDeleted = false
        }
        );

        modelBuilder.Entity<Account>().HasData(new
        {
            AccountId = AccountId.Create(Guid.Parse("c3d4e5f6-3456-7890-cde1-345678901cde")),
            AccountNumber = AccountNumber.Create("1000000001"),
            AccountType = AccountType.Checking, // EF handles enum conversion
            CustomerId = CustomerId.Create(Guid.Parse("a1b2c3d4-1234-5678-9abc-123456789abc")),
            BalanceAmount = 1500.00m,
            Currency = "NGN",
            DateOpened = DateTime.UtcNow.AddDays(-20),
            IsActive = true,
            IsDeleted = false
        });


        modelBuilder.Entity<Account>(entity =>
   {
       entity.Property(a => a.RowVersion)
           .IsRowVersion()
           .IsConcurrencyToken();
   });


        modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(c => c.CustomerId);
                entity.Property(c => c.CustomerId).HasConversion(id => id.Value, value => CustomerId.Create(value));

                // entity.Property(c => c.CustomerId).HasConversion(CustomerId => CustomerId, value => CustomerId.Create());
                entity.Property(c => c.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(c => c.LastName).IsRequired().HasMaxLength(100);
                entity.Property(c => c.Email).IsRequired().HasMaxLength(255);
                entity.Property(c => c.PhoneNumber).HasMaxLength(20);



                entity.HasMany(c => c.Accounts).WithOne(a => a.Customer).HasForeignKey(a => a.CustomerId);
            });

        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(a => a.AccountId);
            entity.Property(a => a.AccountId).HasConversion(id => id.Value, value => AccountId.Create(value));

            // entity.OwnsOne(a => a.AccountNumber, an =>
            // {
            //     an.Property(a => a.Value).HasColumnName("AccountNumber").IsRequired().HasMaxLength(10);
            // });
            entity.Property(a => a.AccountNumber)
            .HasConversion(
            accountNumber => accountNumber.Value,
            value => AccountNumber.Create(value))
            .HasColumnName("AccountNumber")
            .HasMaxLength(10)
            .IsRequired();

            entity.OwnsOne(a => a.Balance, money =>
          {
              money.Property(m => m.Amount).HasColumnName("BalanceAmount").HasPrecision(18, 2);
              money.Property(m => m.Currency).HasColumnName("Currency").HasMaxLength(3).HasDefaultValue("USD");
          });

            entity.Property(a => a.AccountType).HasConversion<string>().IsRequired();

            entity.HasMany(a => a.Transactions).WithOne(t => t.Account).HasForeignKey(t => t.AccountId);
            entity.Navigation(a => a.Transactions).AutoInclude(false);
        });



        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(t => t.TransactionId);
            entity.Property(c => c.TransactionId).HasConversion(id => id.Value, value => TransactionId.Create(value));
            entity.OwnsOne(t => t.Amount, money =>
            {
                money.Property(m => m.Amount).HasColumnName("Amount").HasPrecision(18, 2);
                money.Property(m => m.Currency).HasColumnName("Currency").HasMaxLength(3);
            });
            entity.Property(t => t.Type).HasConversion<string>().IsRequired();
            entity.Property(t => t.Description).HasMaxLength(500);
            entity.Property(t => t.Reference).HasMaxLength(50);
            entity.Property(t => t.Timestamp).IsRequired();
        });



        modelBuilder.Entity<Account>().OwnsOne(a => a.Balance).HasData(
            new
            {
                AccountId = AccountId.Create(Guid.Parse("c3d4e5f6-3456-7890-cde1-345678901cde")),
                Amount = 1500.00m,
                Currency = "NGN"
            }
        );

    }
    public async Task SaveChangesWithOutboxAsync(CancellationToken cancellationToken = default)
    {
        // Convert domain events to outbox messages
        var events = ChangeTracker.Entries<AggregateRoot<AccountId>>()
            .SelectMany(x => x.Entity.DomainEvents)
            .Select(domainEvent => new OutboxMessage
            {
                Id = Guid.NewGuid(),
                Type = domainEvent.GetType().Name,
                Content = JsonSerializer.Serialize(domainEvent, domainEvent.GetType()),
                OccurredOn = domainEvent.OccurredOn
            })
            .ToList();

        // Clear domain events from aggregates
        ChangeTracker.Entries<AggregateRoot<AccountId>>()
            .ToList()
            .ForEach(entry => entry.Entity.ClearDomainEvents());

        // Save changes (including outbox messages) in single transaction
        await base.SaveChangesAsync(cancellationToken);

        // Add outbox messages after saving to ensure they're included in transaction
        if (events.Any())
        {
            await OutboxMessages.AddRangeAsync(events, cancellationToken);
            await base.SaveChangesAsync(cancellationToken);
        }
    }
}

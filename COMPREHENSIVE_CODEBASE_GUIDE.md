# Comprehensive CoreBanking Architecture Guide

## Table of Contents
1. [EXPLAIN LIKE I'M 5 - DDD Concepts Made Simple](#explain-like-im-5---ddd-concepts-made-simple)
2. [Executive Summary](#executive-summary)
3. [Project Structure](#project-structure)
4. [Clean Architecture Overview](#clean-architecture-overview)
5. [Domain Layer Deep Dive](#domain-layer-deep-dive)
6. [Application Layer Deep Dive](#application-layer-deep-dive)
7. [Infrastructure Layer Deep Dive](#infrastructure-layer-deep-dive)
8. [API Layer Deep Dive](#api-layer-deep-dive)
9. [ASP.NET Core Program.cs Explained](#aspnet-core-programcs-explained)
10. [Design Patterns Explained](#design-patterns-explained)
11. [Request Flow Examples](#request-flow-examples)
12. [Mind Maps](#mind-maps)
13. [How to Identify What's What When Designing](#how-to-identify-whats-what-when-designing)

---

## EXPLAIN LIKE I'M 5 - DDD Concepts Made Simple

### The Toy Box Analogy

Imagine you have a **toy box** (your software system). Let's understand each concept using things a 5-year-old knows!

---

### 1. ENTITY = A Toy With a Name Tag

**What is it?**
An Entity is something that has its own **identity** - like YOUR teddy bear that has your name on it.

**Real Life Example:**
```
You have a teddy bear named "Mr. Fluffy"
Your friend has a teddy bear that looks EXACTLY the same
But they're NOT the same bear - yours is YOURS!

Even if you change Mr. Fluffy's clothes, it's still Mr. Fluffy.
Even if you wash him and he looks different, it's still Mr. Fluffy.
The NAME TAG (identity) makes him unique.
```

**In Your Code:**
```csharp
public class Account  // This is an ENTITY
{
    public AccountId Id { get; }  // THIS IS THE NAME TAG!
    public Money Balance { get; }
    public AccountType Type { get; }
}
```

**How to Identify an Entity:**
Ask: "If two things have the same properties, are they the same thing?"
- If NO → It's an Entity (needs identity)
- Two bank accounts with same balance are NOT the same account
- Two customers with same name are NOT the same customer

**Your Entities:**
- `Account` - Each account has unique AccountId
- `Customer` - Each customer has unique CustomerId
- `Transaction` - Each transaction has unique TransactionId

---

### 2. VALUE OBJECT = A Crayon (No Name Tag)

**What is it?**
A Value Object is something where we only care about **what it is**, not **which specific one** it is.

**Real Life Example:**
```
You have a RED crayon.
Your friend has a RED crayon.
Are they the same? YES! They're both just "red crayons"!

You don't care WHICH red crayon you use.
You just care that it's RED.

If the crayon breaks, you throw it away and get another red one.
You don't fix it or track it - you just replace it.
```

**In Your Code:**
```csharp
public record Money  // This is a VALUE OBJECT
{
    public decimal Amount { get; }
    public string Currency { get; }
}

// These are THE SAME because values are equal:
var money1 = new Money(100, "NGN");
var money2 = new Money(100, "NGN");
// money1 == money2 is TRUE!
```

**How to Identify a Value Object:**
Ask: "If two things have the same values, are they interchangeable?"
- If YES → It's a Value Object
- $100 is $100, doesn't matter which specific bill
- "123 Main Street" is the same address regardless of when you wrote it

**Your Value Objects:**
- `Money` - 100 NGN = 100 NGN (we care about amount, not which specific money)
- `AccountId` - Just wraps a GUID (the value matters, not identity of wrapper)
- `AccountNumber` - "1234567890" is just a string value
- `CustomerId` - Just wraps a GUID

**Key Difference:**
```
ENTITY (Account):
  Account #1: Balance = 1000 NGN
  Account #2: Balance = 1000 NGN
  Are they the same? NO! Different accounts!

VALUE OBJECT (Money):
  Money #1: 1000 NGN
  Money #2: 1000 NGN
  Are they the same? YES! Same value!
```

---

### 3. AGGREGATE = A Lego Set (Box of Related Pieces)

**What is it?**
An Aggregate is a **group of things that belong together** and have ONE boss (the Aggregate Root).

**Real Life Example:**
```
You have a LEGO HOUSE set:
┌─────────────────────────┐
│     LEGO HOUSE SET      │  ← The Box (Aggregate)
│  ┌─────────────────┐    │
│  │   INSTRUCTION   │    │  ← Boss (Aggregate Root)
│  │     BOOKLET     │    │
│  └─────────────────┘    │
│                         │
│  🧱 Brick #1            │  ← Parts (Child Entities)
│  🧱 Brick #2            │
│  🧱 Brick #3            │
│  🚪 Door piece          │
│  🪟 Window piece        │
└─────────────────────────┘

Rules:
1. You can't take bricks from this set and put in another set
2. If you want a brick, you ask the INSTRUCTION BOOKLET
3. The booklet knows where every piece should go
4. You don't buy bricks separately - you buy THE SET
```

**In Your Code:**
```csharp
public class Account : AggregateRoot<AccountId>  // Account is the BOSS
{
    public AccountId Id { get; }           // Identity of the boss
    public Money Balance { get; }          // Property
    public Customer Customer { get; }      // Related entity
    public List<Transaction> Transactions { get; }  // Child entities INSIDE the aggregate

    // Only Account can modify its transactions!
    public Result<Transaction> Transfer(Money amount, Account destination)
    {
        // Account controls the rules
        if (Balance < amount)
            return Result.Failure("Not enough money!");

        var transaction = new Transaction(...);  // Account creates transaction
        Transactions.Add(transaction);           // Account manages its children
        return Result.Success(transaction);
    }
}
```

**How to Identify an Aggregate:**
Ask: "What things MUST change together to stay consistent?"

```
Bank Account Aggregate:
┌─────────────────────────────┐
│        ACCOUNT (Root)       │  ← Boss - controls everything
│  ┌───────────────────────┐  │
│  │ Balance: 1000 NGN     │  │
│  │ Status: Active        │  │
│  │ Type: Savings         │  │
│  └───────────────────────┘  │
│                             │
│  Transactions:              │  ← Children - belong to account
│  ├─ Transaction #1          │
│  ├─ Transaction #2          │
│  └─ Transaction #3          │
└─────────────────────────────┘

Rules:
- Can't have Transaction without Account
- Can't modify Transaction directly - go through Account
- Account ensures Balance matches Transactions
```

**Aggregate Root = The Boss**
- Only ONE door into the aggregate (the root)
- All rules enforced by the root
- Outside world talks to root, not children

---

### 4. DOMAIN EVENT = A Newspaper Headline

**What is it?**
A Domain Event is an **announcement** that something important happened. Past tense!

**Real Life Example:**
```
Newspaper Headlines (Domain Events):

📰 "CHILD BORN IN HOSPITAL!"
   - Happened in the past ✓
   - Important news ✓
   - Others might care (grandparents, doctors) ✓

📰 "HOUSE SOLD ON MAIN STREET!"
   - Happened in the past ✓
   - Important news ✓
   - Others might care (neighbors, bank) ✓

When you SHOUT the news:
- You don't know WHO is listening
- You don't care WHO is listening
- Anyone who cares can react
- Grandma might send a gift
- Doctor might schedule checkup
- You just announced it!
```

**In Your Code:**
```csharp
// The announcement (Event)
public record AccountCreatedEvent(
    AccountId AccountId,
    AccountNumber AccountNumber,
    CustomerId CustomerId,
    AccountType AccountType,
    decimal InitialDeposit
) : DomainEvent;

// Who raises it (Account)
public static Account Create(...)
{
    var account = new Account(...);

    // SHOUT THE NEWS!
    account.AddDomainEvent(new AccountCreatedEvent(
        account.Id,
        accountNumber,
        customerId,
        accountType,
        initialDeposit
    ));

    return account;
}

// Who listens (Handlers)
public class AccountCreatedEventHandler : INotificationHandler<AccountCreatedEvent>
{
    public async Task Handle(AccountCreatedEvent notification, ...)
    {
        // React to the news!
        _logger.LogInformation("New account created!");
        // Could also: send email, notify fraud detection, update reports...
    }
}
```

**How to Identify a Domain Event:**
Ask: "Did something important just happen that others might care about?"
- Past tense (happened, not happening)
- Business cares about it
- Other parts of system might react

**Your Domain Events:**
- `AccountCreatedEvent` - "An account WAS created"
- `MoneyTransferedEvent` - "Money WAS transferred"
- `InsufficientFundEvent` - "A transfer WAS rejected"

---

### 5. REPOSITORY = A Librarian

**What is it?**
A Repository is like a **librarian** who finds and stores things for you.

**Real Life Example:**
```
You want a book from the library:

YOU: "I want the book about dinosaurs"
LIBRARIAN: "Let me find it for you"
  - Librarian goes to shelves
  - Searches through books
  - Finds the right one
  - Brings it back
YOU: "Thanks!" (You don't know WHERE it was stored)

You want to return a book:
YOU: "I'm done with this book"
LIBRARIAN: "I'll put it back"
  - Librarian takes book
  - Puts it in right place
  - You don't care where
```

**In Your Code:**
```csharp
public interface IAccountRepository  // Contract with librarian
{
    Task<Account?> GetByIdAsync(AccountId id);  // "Find this account"
    Task<Account?> GetByAccountNumberAsync(AccountNumber number);
    Task AddAsync(Account account);  // "Store this new account"
    Task UpdateAsync(Account account);  // "Update this account"
}

// Using the librarian
public class CreateAccountCommandHandler
{
    private readonly IAccountRepository _repository;  // Our librarian

    public async Task Handle(CreateAccountCommand request, ...)
    {
        var account = Account.Create(...);

        await _repository.AddAsync(account);  // "Please store this"
        // We don't care HOW it's stored (SQL? File? Cloud?)
    }
}
```

**Why Repository?**
- You don't know/care about database details
- Tomorrow you could change from SQL Server to MongoDB
- Your code stays the same!

---

### 6. AGGREGATE ROOT = The Family Spokesperson

**What is it?**
The Aggregate Root is the **ONE person** who speaks for the whole group.

**Real Life Example:**
```
The Johnson Family:
┌─────────────────────────┐
│    DAD (Spokesperson)   │  ← Aggregate Root
│    ├─ Mom               │
│    ├─ Son               │
│    └─ Daughter          │
└─────────────────────────┘

Pizza delivery arrives:
DELIVERY: "Is this the Johnson residence?"
DAD: "Yes, I'll take the pizza"  ← Dad answers for family

NOT like this:
DELIVERY: "Is this the Johnson residence?"
SON: "Yes!"
MOM: "Maybe!"
DAUGHTER: "I think so!"
← Chaos! Everyone answering!

Rules:
1. Outsiders talk to DAD only
2. Dad ensures family follows rules
3. Dad represents the family
```

**In Your Code:**
```csharp
// WRONG - Talking directly to child
transaction.Amount = 500;  // NO! Don't modify transaction directly!

// RIGHT - Talking to Aggregate Root
account.Transfer(money, destination);  // YES! Ask account to do it!
// Account will create/modify transaction internally
```

---

### 7. CLEAN ARCHITECTURE = Layers of an Onion

**What is it?**
Your code organized in **layers**, like an onion. Inner layers don't know about outer layers.

**Real Life Example:**
```
                    🧅 THE ONION 🧅

    ┌─────────────────────────────────────┐
    │         OUTER LAYER (Skin)          │
    │      Can see: Everything inside     │
    │         (API, Controllers)          │
    │    ┌───────────────────────────┐    │
    │    │      MIDDLE LAYER         │    │
    │    │   Can see: Inner only     │    │
    │    │    (Infrastructure)       │    │
    │    │   ┌───────────────────┐   │    │
    │    │   │   INNER LAYER     │   │    │
    │    │   │  Can see: Core    │   │    │
    │    │   │   (Application)   │   │    │
    │    │   │   ┌───────────┐   │   │    │
    │    │   │   │   CORE    │   │   │    │
    │    │   │   │  (Domain) │   │   │    │
    │    │   │   │ Can't see │   │   │    │
    │    │   │   │  outside! │   │   │    │
    │    │   │   └───────────┘   │   │    │
    │    │   └───────────────────┘   │    │
    │    └───────────────────────────┘    │
    └─────────────────────────────────────┘
```

**Your Code:**
```
CoreBanking.Core (Center - Domain)
├─ Knows: NOTHING about outside world
├─ Contains: Account, Money, Events, Rules
└─ Pure business logic

CoreBanking.APP (Application)
├─ Knows: Domain layer only
├─ Contains: Commands, Queries, Handlers
└─ Uses domain to do things

CoreBanking.Infrastructure (Infrastructure)
├─ Knows: Domain + Application
├─ Contains: Database, Service Bus, Repositories
└─ Implements interfaces from inner layers

CoreBanking.API (Outer - Presentation)
├─ Knows: Everything inside
├─ Contains: Controllers, SignalR Hubs
└─ Entry point for outside world
```

**Why This Structure?**
- Domain (Account) doesn't know about SQL Server
- If you change database, Domain code stays same
- Business rules protected in the center
- Easy to test (mock outer layers)

---

### 8. CQRS = Different Lines at the Store

**What is it?**
**C**ommand **Q**uery **R**esponsibility **S**egregation = Separate paths for reading and writing.

**Real Life Example:**
```
At the grocery store:

RETURNS LINE (Command - Changes things):
┌─────────────────┐
│ "I want to      │
│  return this"   │  ← Changes inventory
│                 │  ← Changes your money
│ Takes longer    │  ← More rules to follow
└─────────────────┘

PRICE CHECK LINE (Query - Just looking):
┌─────────────────┐
│ "How much is    │
│  this apple?"   │  ← Just reading price
│                 │  ← Doesn't change anything
│ Quick answer    │  ← Simple, fast
└─────────────────┘
```

**In Your Code:**
```csharp
// COMMAND (Changes state)
public record TransferMoneyCommand(
    string SourceAccount,
    string DestinationAccount,
    decimal Amount
) : IRequest<Result>;  // Changes balances!

// QUERY (Just reading)
public record GetAccountDetailsQuery(
    string AccountNumber
) : IRequest<AccountDetailsDto>;  // Just returns data!
```

**Why Separate?**
- Queries are simple and fast
- Commands have business rules
- Can optimize each separately
- Easier to understand

---

### 9. MEDIATOR (MediatR) = The Teacher in Class

**What is it?**
The Mediator is the **middleman** who passes messages. Students don't talk directly to each other.

**Real Life Example:**
```
WITHOUT Teacher (Mediator):
┌─────┐    ┌─────┐    ┌─────┐
│ Kid │───→│ Kid │───→│ Kid │
│  A  │←───│  B  │←───│  C  │
└─────┘    └─────┘    └─────┘
Everyone talking to everyone = CHAOS!

WITH Teacher (Mediator):
┌─────┐         ┌─────┐
│ Kid │         │ Kid │
│  A  │         │  B  │
└──┬──┘         └──┬──┘
   │               │
   └──────┬────────┘
          ↓
    ┌──────────┐
    │ TEACHER  │
    │(Mediator)│
    └──────────┘
          ↓
    ┌──────────┐
    │ Kid C    │
    └──────────┘

Kid A: "Teacher, I have a question"
Teacher: "Let me find who can answer"
Teacher asks Kid C
Kid C answers through Teacher
```

**In Your Code:**
```csharp
// WITHOUT MediatR
public class AccountsController
{
    private readonly CreateAccountHandler _createHandler;
    private readonly TransferMoneyHandler _transferHandler;
    private readonly GetAccountHandler _getHandler;
    // ... knows about ALL handlers!
}

// WITH MediatR
public class AccountsController
{
    private readonly IMediator _mediator;  // Only knows mediator!

    public async Task<ActionResult> CreateAccount(CreateAccountRequest request)
    {
        var command = new CreateAccountCommand(...);
        var result = await _mediator.Send(command);  // "Teacher, handle this please"
        // Controller doesn't know WHO handles it!
    }
}
```

---

### 10. UNIT OF WORK = The Save Button

**What is it?**
Unit of Work is like clicking **SAVE** - all your changes are saved together, or none are saved.

**Real Life Example:**
```
You're editing a Word document:

1. Type some text
2. Add a picture
3. Change font color
4. Delete a paragraph

Nothing is saved to file yet!

Then you click SAVE:
- ALL changes saved at once
- Either everything saves, or nothing saves
- If computer crashes during save, file not corrupted
```

**In Your Code:**
```csharp
public class TransferMoneyCommandHandler
{
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;  // The SAVE button

    public async Task Handle(TransferMoneyCommand request, ...)
    {
        var source = await _accountRepository.GetByIdAsync(...);
        var destination = await _accountRepository.GetByIdAsync(...);

        // Make changes (not saved yet!)
        source.Transfer(amount, destination);

        await _accountRepository.UpdateAsync(source);       // Mark for saving
        await _accountRepository.UpdateAsync(destination);  // Mark for saving

        // NOW click SAVE - both accounts update together!
        await _unitOfWork.SaveChangesAsync();

        // If something fails, BOTH accounts roll back
        // Money doesn't disappear!
    }
}
```

---

### Visual Summary: The Toy Store

```
🏪 THE TOY STORE (Your Banking System)

┌─────────────────────────────────────────────────────────┐
│                    FRONT DESK (API Layer)               │
│  "Welcome! What do you need?"                           │
│  - REST Controllers (web requests)                      │
│  - SignalR (real-time updates)                         │
│  - gRPC (fast communication)                           │
└────────────────────────┬────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────┐
│              CUSTOMER SERVICE (Application Layer)        │
│  "Let me process your request"                          │
│  - Commands: "I want to buy a toy"                      │
│  - Queries: "Show me available toys"                    │
│  - Handlers: Process the request                        │
│  - Mediator: Routes to right person                     │
└────────────────────────┬────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────┐
│              WAREHOUSE (Infrastructure Layer)            │
│  "I'll get it from storage"                             │
│  - Repository: Finds/stores toys                        │
│  - Database: Where toys are kept                        │
│  - Service Bus: Talks to other stores                   │
└────────────────────────┬────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────┐
│                 THE TOYS (Domain Layer)                  │
│  "We are the actual products"                           │
│                                                         │
│  ENTITY (Toy with name tag):                            │
│  🧸 "Mr. Fluffy" - Unique teddy bear                    │
│                                                         │
│  VALUE OBJECT (Just a color):                           │
│  🔴 Red - Any red is the same                           │
│                                                         │
│  AGGREGATE (Toy set in a box):                          │
│  📦 LEGO Set - Box is the root, bricks inside          │
│                                                         │
│  DOMAIN EVENT (Announcement):                           │
│  📰 "NEW TOY ARRIVED!" - Others react                   │
└─────────────────────────────────────────────────────────┘
```

---

### Quick Reference Card

| Concept | Simple Analogy | Your Code Example | Key Question to Identify |
|---------|---------------|-------------------|-------------------------|
| **Entity** | Toy with name tag | `Account`, `Customer` | "Do I need to track THIS specific one?" |
| **Value Object** | A crayon color | `Money`, `AccountId` | "Do I only care about the value?" |
| **Aggregate** | LEGO set in box | `Account` with `Transactions` | "What must change together?" |
| **Aggregate Root** | Family spokesperson | `Account` class | "Who's the boss of this group?" |
| **Domain Event** | Newspaper headline | `AccountCreatedEvent` | "Did something important happen?" |
| **Repository** | Librarian | `IAccountRepository` | "Where do I store/find things?" |
| **Unit of Work** | Save button | `IUnitOfWork` | "When do all changes commit?" |
| **Command** | "Do this!" | `CreateAccountCommand` | "Does this change something?" |
| **Query** | "Tell me about..." | `GetAccountDetailsQuery` | "Does this just read data?" |
| **Mediator** | Teacher in class | `IMediator` | "Who routes my request?" |

---



This is a **production-ready banking system** built using:
- **Clean Architecture** - Separation of concerns into layers
- **DDD (Domain-Driven Design)** - Rich domain model with business logic
- **CQRS (Command Query Responsibility Segregation)** - Separate read/write paths
- **MediatR** - In-process messaging/mediator pattern
- **Event-Driven Architecture** - Domain events + Azure Service Bus
- **Outbox Pattern** - Reliable event publishing
- **SignalR** - Real-time notifications
- **Hangfire** - Background job scheduling
- **gRPC** - High-performance APIs alongside REST

---

## Project Structure

```
CoreBanking/
├── CoreBanking.Core/              # Domain Layer (innermost)
│   ├── Common/                    # Base classes (AggregateRoot, DomainEvent)
│   ├── Entities/                  # Domain entities (Account, Customer, Transaction)
│   ├── ValueObjects/              # Value objects (Money, AccountId, CustomerId)
│   ├── Events/                    # Domain events
│   ├── Interfaces/                # Contracts (IRepository, IUnitOfWork)
│   └── Enums/                     # Domain enumerations
│
├── CoreBanking.APP/               # Application Layer
│   ├── Accounts/
│   │   ├── Commands/              # Write operations (CreateAccount, TransferMoney)
│   │   ├── Queries/               # Read operations (GetAccountDetails)
│   │   └── EventHandlers/         # React to domain events
│   ├── Customers/
│   │   ├── Commands/
│   │   └── Queries/
│   ├── Common/
│   │   ├── Behaviors/             # Pipeline behaviors (Logging, Validation)
│   │   ├── Interfaces/            # Application contracts
│   │   └── Models/                # DTOs, Result pattern
│   └── Mappings/                  # AutoMapper profiles
│
├── CoreBanking.Infrastructure/    # Infrastructure Layer
│   ├── Data/                      # EF Core DbContext
│   ├── Repositories/              # Data access implementations
│   ├── ServiceBus/                # Azure Service Bus integration
│   ├── BackgroundJobs/            # Hangfire configuration
│   ├── Persistence/
│   │   └── Outbox/                # Outbox pattern implementation
│   ├── Resilience/                # Polly policies
│   └── Migrations/                # Database migrations
│
├── CoreBanking.API/               # Presentation Layer (outermost)
│   ├── Controllers/               # REST API endpoints
│   ├── gRPC/
│   │   ├── Services/              # gRPC service implementations
│   │   └── Protos/                # Protocol buffer definitions
│   ├── Hubs/                      # SignalR hubs
│   ├── Middleware/                # Global exception handling
│   └── Extensions/                # Service registration extensions
│
└── CoreBanking.GrpcClient/        # gRPC client example
```

---

## Clean Architecture Overview

### The Dependency Rule

```
┌─────────────────────────────────────────────────────┐
│                    API Layer                         │
│         (Controllers, gRPC, SignalR, Middleware)    │
├─────────────────────────────────────────────────────┤
│               Infrastructure Layer                   │
│    (Database, ServiceBus, Repositories, Hangfire)   │
├─────────────────────────────────────────────────────┤
│               Application Layer                      │
│        (Commands, Queries, Handlers, Behaviors)     │
├─────────────────────────────────────────────────────┤
│                 Domain Layer                         │
│      (Entities, Value Objects, Domain Events)       │
└─────────────────────────────────────────────────────┘
```

**Key Principle**: Dependencies point INWARD. The Domain layer knows nothing about the outer layers.

### Why This Architecture?

1. **Testability** - Each layer can be tested in isolation
2. **Maintainability** - Changes in one layer don't affect others
3. **Flexibility** - Easy to swap implementations (e.g., change database)
4. **Business Logic Protection** - Core business rules are isolated

---

## Domain Layer Deep Dive

### 1. Entities (Objects with Identity)

**Account.cs** - The main aggregate root
```csharp
public class Account : AggregateRoot<AccountId>
{
    // Private fields - encapsulation
    private AccountNumber _accountNumber;
    private Money _balance;
    private AccountType _type;
    private bool _isActive;

    // Factory method - controlled creation
    public static Account Create(
        CustomerId customerId,
        AccountNumber accountNumber,
        AccountType type,
        Money initialBalance)
    {
        // Business validation
        if (initialBalance.Amount < 0)
            throw new DomainException("Initial balance cannot be negative");

        if (initialBalance.Amount > 1_000_000)
            throw new DomainException("Initial balance exceeds maximum");

        var account = new Account
        {
            Id = AccountId.Create(),
            _accountNumber = accountNumber,
            _balance = initialBalance,
            _type = type,
            _isActive = true
        };

        // Raise domain event
        account.AddDomainEvent(new AccountCreatedEvent(
            account.Id,
            accountNumber,
            customerId,
            type,
            initialBalance.Amount
        ));

        return account;
    }

    // Business operation with rules
    public Result<Transaction> Transfer(
        Money amount,
        Account destination,
        string reference,
        string description)
    {
        // Business rule: Account must be active
        if (!_isActive)
            return Result<Transaction>.Failure("Source account is inactive");

        if (!destination._isActive)
            return Result<Transaction>.Failure("Destination account is inactive");

        // Business rule: Sufficient balance
        if (_balance.Amount < amount.Amount)
        {
            AddDomainEvent(new InsufficientFundEvent(
                _accountNumber,
                amount.Amount,
                _balance.Amount,
                "Transfer failed"
            ));
            return Result<Transaction>.Failure("Insufficient funds");
        }

        // Business rule: Savings account withdrawal limit
        if (_type == AccountType.Savings && GetMonthlyWithdrawals() >= 6)
            return Result<Transaction>.Failure("Savings account withdrawal limit reached");

        // Execute transfer
        _balance -= amount;
        destination._balance += amount;

        var transaction = Transaction.Create(
            Id, amount, TransactionType.Transfer, reference, description);

        // Raise domain event
        AddDomainEvent(new MoneyTransferedEvent(
            transaction.Id,
            _accountNumber,
            destination._accountNumber,
            amount.Amount,
            reference
        ));

        return Result<Transaction>.Success(transaction);
    }
}
```

**Line-by-Line Explanation:**
- **Line 1**: Inherits from `AggregateRoot<AccountId>` - this is the root of the aggregate
- **Lines 3-6**: Private fields ensure encapsulation - no external modification
- **Line 9**: Factory method (`Create`) - only way to create an account
- **Lines 12-15**: Business validation - domain rules enforced at creation
- **Lines 17-24**: Object construction with required fields
- **Lines 26-32**: Domain event raised - tells the system "something happened"
- **Line 37**: `Transfer` method - business operation with rules
- **Lines 40-45**: Business rules checked (active accounts only)
- **Lines 47-55**: Insufficient funds check with event
- **Lines 57-58**: Savings account specific rule
- **Lines 61-62**: The actual transfer logic
- **Lines 64-65**: Transaction record creation
- **Lines 67-72**: Domain event for successful transfer

### 2. Value Objects (Objects without Identity)

**Money.cs** - Immutable value object
```csharp
public record Money
{
    public decimal Amount { get; init; }
    public string Currency { get; init; }

    public Money(decimal amount, string currency)
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative");

        if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
            throw new ArgumentException("Currency must be 3-letter ISO code");

        Amount = amount;
        Currency = currency.ToUpperInvariant();
    }

    // Operator overloading for natural usage
    public static Money operator +(Money a, Money b)
    {
        if (a.Currency != b.Currency)
            throw new InvalidOperationException("Cannot add different currencies");

        return new Money(a.Amount + b.Amount, a.Currency);
    }

    public static Money operator -(Money a, Money b)
    {
        if (a.Currency != b.Currency)
            throw new InvalidOperationException("Cannot subtract different currencies");

        return new Money(a.Amount - b.Amount, a.Currency);
    }
}
```

**Why Value Objects?**
- **Immutability** - Can't be changed after creation (thread-safe)
- **Self-validation** - Always in valid state
- **Equality by value** - Two Money objects with same amount/currency are equal
- **Rich behavior** - Operators make code readable: `balance -= transferAmount`

### 3. Domain Events (What Happened)

**AccountCreatedEvent.cs**
```csharp
public record AccountCreatedEvent(
    AccountId AccountId,
    AccountNumber AccountNumber,
    CustomerId CustomerId,
    AccountType AccountType,
    decimal InitialDeposit
) : DomainEvent;
```

**MoneyTransferedEvent.cs**
```csharp
public record MoneyTransferedEvent(
    TransactionId TransactionId,
    AccountNumber SourceAccountNumber,
    AccountNumber DestinationAccountNumber,
    decimal Amount,
    string Reference
) : DomainEvent;
```

**Why Domain Events?**
- **Decoupling** - Account doesn't know who cares about the transfer
- **Audit trail** - Record of what happened
- **Extensibility** - Add new handlers without changing domain
- **Event-driven architecture** - Foundation for microservices

### 4. Aggregate Root Base Class

**AggregateRoot.cs**
```csharp
public abstract class AggregateRoot<TId> : IAggregateRoot
{
    public TId Id { get; protected set; }

    private readonly List<DomainEvent> _domainEvents = new();
    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(DomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
```

**Why Aggregate Roots?**
- **Consistency boundary** - All changes go through the root
- **Transaction boundary** - One aggregate = one transaction
- **Event collection** - Gathers all events for publishing

---

## Application Layer Deep Dive

### 1. Commands (Write Operations)

**CreateAccountCommand.cs**
```csharp
public record CreateAccountCommand(
    Guid CustomerId,
    string AccountType,
    decimal InitialDeposit,
    string Currency = "NGN"
) : IRequest<Result<Guid>>;
```

**CreateAccountCommandHandler.cs**
```csharp
public class CreateAccountCommandHandler : IRequestHandler<CreateAccountCommand, Result<Guid>>
{
    private readonly IAccountRepository _accountRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateAccountCommandHandler> _logger;

    public CreateAccountCommandHandler(
        IAccountRepository accountRepository,
        ICustomerRepository customerRepository,
        IUnitOfWork unitOfWork,
        ILogger<CreateAccountCommandHandler> logger)
    {
        _accountRepository = accountRepository;
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(
        CreateAccountCommand request,
        CancellationToken cancellationToken)
    {
        // Step 1: Validate customer exists
        var customer = await _customerRepository.GetByIdAsync(
            new CustomerId(request.CustomerId), cancellationToken);

        if (customer is null)
            return Result<Guid>.Failure("Customer not found");

        // Step 2: Generate unique account number
        var accountNumber = await GenerateUniqueAccountNumberAsync(cancellationToken);

        // Step 3: Parse account type
        if (!Enum.TryParse<AccountType>(request.AccountType, out var accountType))
            return Result<Guid>.Failure("Invalid account type");

        // Step 4: Create account via domain factory
        var account = Account.Create(
            new CustomerId(request.CustomerId),
            accountNumber,
            accountType,
            new Money(request.InitialDeposit, request.Currency)
        );

        // Step 5: Persist to database
        await _accountRepository.AddAsync(account, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Created account {AccountNumber} for customer {CustomerId}",
            accountNumber, request.CustomerId);

        // Step 6: Return result
        return Result<Guid>.Success(account.Id.Value);
    }

    private async Task<AccountNumber> GenerateUniqueAccountNumberAsync(
        CancellationToken cancellationToken)
    {
        AccountNumber accountNumber;
        do
        {
            accountNumber = AccountNumber.Generate();
        } while (await _accountRepository.AccountNumberExistsAsync(
            accountNumber, cancellationToken));

        return accountNumber;
    }
}
```

**Line-by-Line Explanation:**
- **Lines 1-6**: Command is a simple record with data needed for operation
- **Line 7**: `IRequest<Result<Guid>>` - MediatR interface, returns Result with account ID
- **Lines 9-19**: Handler with dependencies injected via constructor
- **Line 21**: `Handle` method - the actual use case implementation
- **Lines 24-28**: Validates that customer exists before creating account
- **Line 31**: Generates unique account number
- **Lines 34-35**: Parses account type enum safely
- **Lines 38-43**: Calls domain factory method (business logic lives in domain)
- **Lines 46-47**: Persists to database (repository pattern)
- **Line 51**: Returns success with account ID

### 2. Queries (Read Operations)

**GetAccountDetailsQuery.cs**
```csharp
public record GetAccountDetailsQuery(string AccountNumber) : IRequest<Result<AccountDetailsDto>>;
```

**GetAccountDetailsQueryHandler.cs**
```csharp
public class GetAccountDetailsQueryHandler
    : IRequestHandler<GetAccountDetailsQuery, Result<AccountDetailsDto>>
{
    private readonly IAccountRepository _accountRepository;

    public async Task<Result<AccountDetailsDto>> Handle(
        GetAccountDetailsQuery request,
        CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetByAccountNumberAsync(
            new AccountNumber(request.AccountNumber), cancellationToken);

        if (account is null)
            return Result<AccountDetailsDto>.Failure("Account not found");

        var dto = new AccountDetailsDto
        {
            AccountNumber = account.AccountNumber.Value,
            Balance = account.Balance.Amount,
            Currency = account.Balance.Currency,
            AccountType = account.Type.ToString(),
            IsActive = account.IsActive,
            CustomerId = account.CustomerId.Value
        };

        return Result<AccountDetailsDto>.Success(dto);
    }
}
```

**Why Separate Commands and Queries (CQRS)?**
- **Different models** - Reads can be optimized separately from writes
- **Scalability** - Can scale read/write databases independently
- **Simplicity** - Each handler does one thing
- **Performance** - Queries can bypass domain model for speed

### 3. Pipeline Behaviors (Cross-Cutting Concerns)

**LoggingBehavior.cs** - Logs all commands/queries
```csharp
public class LoggingBehaviour<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehaviour<TRequest, TResponse>> _logger;

    public LoggingBehaviour(ILogger<LoggingBehaviour<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // BEFORE: Log what's being handled
        var requestname = typeof(TRequest).Name;
        _logger.LogInformation(
            "Handling command {CommandName} with payload {@Request}",
            requestname, request);

        // Start timer
        var timer = System.Diagnostics.Stopwatch.StartNew();

        // Call next behavior or handler
        var response = await next();

        // AFTER: Log completion time
        timer.Stop();
        _logger.LogInformation(
            "Command {CommandName} handled in {ElapsedMilliseconds}ms",
            requestname, timer.ElapsedMilliseconds);

        return response;
    }
}
```

**ValidationBehavior.cs** - Validates all requests
```csharp
public class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Skip if no validators
        if (!_validators.Any())
            return await next();

        // Run all validators
        var context = new ValidationContext<TRequest>(request);
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        // Collect all errors
        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        // If errors, return failure
        if (failures.Count != 0)
        {
            var errors = failures.Select(f => f.ErrorMessage).ToArray();
            // Create failure result using reflection or type checking
            // This assumes TResponse is Result<T>
            throw new ValidationException(failures);
        }

        // If valid, continue pipeline
        return await next();
    }
}
```

**DomainEventsBehavior.cs** - Dispatches domain events after handler completes
```csharp
public class DomainEventsBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IDomainEventDispatcher _dispatcher;
    private readonly ILogger<DomainEventsBehavior<TRequest, TResponse>> _logger;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing domain events for {RequestType}",
            typeof(TRequest).Name);

        // Execute the handler
        var response = await next();

        // After handler completes, dispatch domain events
        await _dispatcher.DispatchDomainEventsAsync(cancellationToken);

        return response;
    }
}
```

**Pipeline Execution Order:**
```
Request comes in
    ↓
DomainEventsBehavior (wraps everything)
    ↓
ValidationBehavior (validates input)
    ↓
LoggingBehavior (logs timing)
    ↓
Actual Handler (executes use case)
    ↓
Response returns up the chain
    ↓
Domain events dispatched
    ↓
Response returned to caller
```

### 4. Event Handlers (React to Events)

**AccountCreatedEventHandler.cs**
```csharp
public class AccountCreatedEventHandler : INotificationHandler<AccountCreatedEvent>
{
    private readonly ILogger<AccountCreatedEventHandler> _logger;

    public async Task Handle(
        AccountCreatedEvent notification,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing account created event for account {AccountNumber}",
            notification.AccountNumber);

        // Could send welcome email, notify fraud detection, etc.
        await Task.CompletedTask;
    }
}
```

**RealTimeNotificationEventHandler.cs**
```csharp
public class RealTimeNotificationEventHandler : INotificationHandler<MoneyTransferedEvent>
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public async Task Handle(
        MoneyTransferedEvent notification,
        CancellationToken cancellationToken)
    {
        // Broadcast to SignalR clients
        await _hubContext.Clients
            .Group($"account-{notification.SourceAccountNumber}")
            .SendAsync("TransactionCompleted", new
            {
                notification.TransactionId,
                notification.Amount,
                notification.Reference,
                Type = "Debit"
            }, cancellationToken);

        await _hubContext.Clients
            .Group($"account-{notification.DestinationAccountNumber}")
            .SendAsync("TransactionCompleted", new
            {
                notification.TransactionId,
                notification.Amount,
                notification.Reference,
                Type = "Credit"
            }, cancellationToken);
    }
}
```

---

## Infrastructure Layer Deep Dive

### 1. Entity Framework Core DbContext

**BankingDbContext.cs**
```csharp
public class BankingDbContext : DbContext
{
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Account> Accounts { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<OutboxMessage> OutboxMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Account Configuration
        modelBuilder.Entity<Account>(entity =>
        {
            // Primary key conversion
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Id)
                .HasConversion(
                    id => id.Value,           // To database
                    value => new AccountId(value)  // From database
                );

            // Value object - Account Number
            entity.Property(a => a.AccountNumber)
                .HasConversion(
                    an => an.Value,
                    value => new AccountNumber(value)
                )
                .HasMaxLength(20)
                .IsRequired();

            // Owned type - Money (stored as two columns)
            entity.OwnsOne(a => a.Balance, money =>
            {
                money.Property(m => m.Amount)
                    .HasColumnName("Balance")
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                money.Property(m => m.Currency)
                    .HasColumnName("Currency")
                    .HasMaxLength(3)
                    .IsRequired();
            });

            // Enum stored as string
            entity.Property(a => a.Type)
                .HasConversion<string>()
                .HasMaxLength(20);

            // Concurrency token
            entity.Property(a => a.RowVersion)
                .IsRowVersion();

            // Relationships
            entity.HasOne(a => a.Customer)
                .WithMany(c => c.Accounts)
                .HasForeignKey(a => a.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(a => a.Transactions)
                .WithOne(t => t.Account)
                .HasForeignKey(t => t.AccountId);

            // Global query filter - soft delete
            entity.HasQueryFilter(a => !a.IsDeleted);

            // Index
            entity.HasIndex(a => a.AccountNumber).IsUnique();
        });

        // Seed data
        modelBuilder.Entity<Customer>().HasData(
            new { Id = new CustomerId(Guid.Parse("...")), ... }
        );

        modelBuilder.Entity<Account>().HasData(
            new { ... InitialBalance = 1500m, Currency = "NGN" }
        );
    }
}
```

**Key Concepts:**
- **HasConversion** - Converts value objects to/from database types
- **OwnsOne** - Maps value object as columns in same table
- **HasQueryFilter** - Automatically filters soft-deleted records
- **IsRowVersion** - Optimistic concurrency control
- **HasData** - Seed data for initial setup

### 2. Repository Pattern

**AccountRepository.cs**
```csharp
public class AccountRepository : IAccountRepository
{
    private readonly BankingDbContext _context;

    public AccountRepository(BankingDbContext context)
    {
        _context = context;
    }

    public async Task<Account?> GetByIdAsync(
        AccountId id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Accounts
            .Include(a => a.Customer)
            .Include(a => a.Transactions)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<Account?> GetByAccountNumberAsync(
        AccountNumber accountNumber,
        CancellationToken cancellationToken = default)
    {
        return await _context.Accounts
            .Include(a => a.Customer)
            .Include(a => a.Transactions)
            .FirstOrDefaultAsync(
                a => a.AccountNumber == accountNumber,
                cancellationToken);
    }

    public async Task AddAsync(
        Account account,
        CancellationToken cancellationToken = default)
    {
        await _context.Accounts.AddAsync(account, cancellationToken);
    }

    public Task UpdateAsync(
        Account account,
        CancellationToken cancellationToken = default)
    {
        _context.Accounts.Update(account);
        return Task.CompletedTask;
    }

    public async Task<bool> AccountNumberExistsAsync(
        AccountNumber accountNumber,
        CancellationToken cancellationToken = default)
    {
        return await _context.Accounts
            .AnyAsync(a => a.AccountNumber == accountNumber, cancellationToken);
    }
}
```

**Why Repository Pattern?**
- **Abstraction** - Domain doesn't know about EF Core
- **Testability** - Can mock repository in tests
- **Single Responsibility** - Repository only handles data access
- **Query encapsulation** - Complex queries hidden behind simple methods

### 3. Unit of Work Pattern

**UnitOfWork.cs**
```csharp
public class UnitOfWork : IUnitOfWork
{
    private readonly BankingDbContext _context;

    public UnitOfWork(BankingDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
```

**Why Unit of Work?**
- **Transaction boundary** - All changes saved together
- **Atomic operations** - Either all succeed or all fail
- **Separation** - Repository adds to tracker, UoW saves

### 4. Outbox Pattern (Reliable Event Publishing)

**OutboxMessage.cs**
```csharp
public class OutboxMessage
{
    public Guid Id { get; set; }
    public string Type { get; set; }           // Event type name
    public string Content { get; set; }        // JSON serialized event
    public DateTime OccurredOn { get; set; }
    public DateTime? ProcessedOn { get; set; } // Null = not processed
    public string? Error { get; set; }
    public int RetryCount { get; set; }
}
```

**OutboxMessageProcessor.cs**
```csharp
public class OutboxMessageProcessor : IOutboxMessageProcessor
{
    private readonly BankingDbContext _context;
    private readonly IServiceBusEventPublisher _eventPublisher;

    public async Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken)
    {
        // Get unprocessed messages
        var messages = await _context.OutboxMessages
            .Where(m => m.ProcessedOn == null && m.RetryCount < 3)
            .OrderBy(m => m.OccurredOn)
            .Take(100)
            .ToListAsync(cancellationToken);

        foreach (var message in messages)
        {
            try
            {
                // Deserialize event
                var eventType = Type.GetType(message.Type);
                var domainEvent = JsonSerializer.Deserialize(
                    message.Content, eventType) as DomainEvent;

                // Publish to service bus
                await _eventPublisher.PublishAsync(domainEvent);

                // Mark as processed
                message.ProcessedOn = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                message.Error = ex.Message;
                message.RetryCount++;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
```

**Why Outbox Pattern?**
- **Reliability** - Events stored in same transaction as data
- **Consistency** - No lost events if service bus is down
- **Retry mechanism** - Failed publishes retry automatically
- **Audit trail** - All events logged in database

### 5. Service Bus Integration

**ServiceBusEventPublisher.cs**
```csharp
public class ServiceBusEventPublisher : IEventPublisher
{
    private readonly IBankingServiceBusSender _sender;

    public async Task PublishAsync<TEvent>(TEvent domainEvent)
        where TEvent : DomainEvent
    {
        var topicName = GetTopicNameForEvent(domainEvent);

        var messageData = CreateMessageData(domainEvent);

        var properties = new Dictionary<string, object>
        {
            ["EventType"] = domainEvent.GetType().Name,
            ["EventId"] = domainEvent.Id.ToString(),
            ["OccurredOn"] = domainEvent.OccurredOn.ToString("O"),
            ["Source"] = "CoreBanking",
            ["Version"] = "1.0"
        };

        await _sender.SendMessageAsync(topicName, messageData, properties);
    }

    private string GetTopicNameForEvent(DomainEvent domainEvent)
    {
        return domainEvent switch
        {
            AccountCreatedEvent => "account-events",
            MoneyTransferedEvent => "transaction-events",
            CustomerCreatedEvent => "customer-events",
            _ => "general-events"
        };
    }
}
```

---

## API Layer Deep Dive

### 1. REST Controllers

**AccountsController.cs**
```csharp
[ApiController]
[Route("api/[controller]")]
public class AccountsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public AccountsController(IMediator mediator, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }

    [HttpGet("{accountNumber}")]
    public async Task<ActionResult<ApiResponse<AccountDetailsDto>>> GetAccountDetails(
        string accountNumber)
    {
        var query = new GetAccountDetailsQuery(accountNumber);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
            return NotFound(ApiResponse<AccountDetailsDto>.CreateFailure(result.Error));

        return Ok(ApiResponse<AccountDetailsDto>.CreateSuccess(result.Value));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateAccount(
        [FromBody] CreateAccountRequest request)
    {
        var command = _mapper.Map<CreateAccountCommand>(request);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse<Guid>.CreateFailure(result.Error));

        return CreatedAtAction(
            nameof(GetAccountDetails),
            new { accountNumber = "..." },
            ApiResponse<Guid>.CreateSuccess(result.Value));
    }

    [HttpPost("{accountNumber}/transfer")]
    public async Task<ActionResult<ApiResponse<TransactionDto>>> TransferMoney(
        string accountNumber,
        [FromBody] TransferMoneyRequest request)
    {
        var command = new TransferMoneyCommand(
            accountNumber,
            request.DestinationAccountNumber,
            request.Amount,
            request.Currency);

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse<TransactionDto>.CreateFailure(result.Error));

        return Ok(ApiResponse<TransactionDto>.CreateSuccess(result.Value));
    }
}
```

**Controller Responsibilities:**
- Receive HTTP requests
- Map to commands/queries
- Send to MediatR
- Map results to HTTP responses
- NO business logic here!

### 2. SignalR Hubs (Real-Time Communication)

**NotificationHub.cs**
```csharp
public class NotificationHub : Hub
{
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(ILogger<NotificationHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client {ConnectionId} connected", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client {ConnectionId} disconnected", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    // Client calls this to subscribe to account updates
    public async Task SubscribeToAccount(string accountNumber)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"account-{accountNumber}");
        _logger.LogInformation(
            "Client {ConnectionId} subscribed to account {AccountNumber}",
            Context.ConnectionId, accountNumber);
    }

    // Client calls this to unsubscribe
    public async Task UnsubscribeFromAccount(string accountNumber)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"account-{accountNumber}");
    }
}
```

**SignalR Concepts:**
- **Hub** - Server-side endpoint for real-time communication
- **Groups** - Logical grouping of connections
- **Context.ConnectionId** - Unique identifier for each connection
- **Clients.Group()** - Send message to all connections in group

### 3. Global Exception Handler Middleware

**GlobalExceptionHandlerMiddleware.cs**
```csharp
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error occurred");
            await HandleExceptionAsync(context, ex, HttpStatusCode.BadRequest);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Resource not found");
            await HandleExceptionAsync(context, ex, HttpStatusCode.NotFound);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation");
            await HandleExceptionAsync(context, ex, HttpStatusCode.Conflict);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred");
            await HandleExceptionAsync(context, ex, HttpStatusCode.InternalServerError);
        }
    }

    private static async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception,
        HttpStatusCode statusCode)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = ApiResponse<object>.CreateFailure(exception.Message);

        var json = JsonSerializer.Serialize(response);
        await context.Response.WriteAsync(json);
    }
}
```

**Why Middleware?**
- **Centralized error handling** - One place for all exceptions
- **Consistent responses** - All errors follow same format
- **Cross-cutting concern** - Applied to all requests
- **Clean controllers** - No try-catch blocks needed

---

## ASP.NET Core Program.cs Explained

This is the heart of your application's configuration. Let me explain every concept:

### 1. Dependency Injection (DI) Container

```csharp
var builder = WebApplication.CreateBuilder(args);

// Register services in the DI container
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddSingleton<IServiceBusClientFactory, ServiceBusClientFactory>();
builder.Services.AddTransient<INotificationHandler<AccountCreatedEvent>, AccountCreatedEventHandler>();
```

**What is Dependency Injection?**
Dependency Injection is a design pattern where objects receive their dependencies from external sources rather than creating them internally.

**Without DI:**
```csharp
public class AccountsController
{
    private readonly AccountRepository _repository;

    public AccountsController()
    {
        // Controller creates its own dependencies - BAD!
        var connectionString = "...";
        var options = new DbContextOptions<BankingDbContext>();
        var context = new BankingDbContext(options);
        _repository = new AccountRepository(context);
    }
}
```

**With DI:**
```csharp
public class AccountsController
{
    private readonly IAccountRepository _repository;

    public AccountsController(IAccountRepository repository)
    {
        // Dependencies injected - GOOD!
        _repository = repository;
    }
}
```

**Why DI?**
1. **Loose coupling** - Controller doesn't know about AccountRepository implementation
2. **Testability** - Can inject mock repository for testing
3. **Maintainability** - Change implementation without modifying consumers
4. **Lifetime management** - Container manages object creation and disposal

### 2. Service Lifetimes

```csharp
// Scoped - One instance per HTTP request
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<BankingDbContext>();

// Singleton - One instance for entire application lifetime
builder.Services.AddSingleton<IServiceBusClientFactory, ServiceBusClientFactory>();
builder.Services.AddSingleton<ConnectionStateService>();

// Transient - New instance every time it's requested
builder.Services.AddTransient<INotificationHandler<AccountCreatedEvent>, AccountCreatedEventHandler>();
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
```

**Scoped Lifetime:**
```
HTTP Request 1:                    HTTP Request 2:
┌─────────────────┐               ┌─────────────────┐
│ Controller A    │               │ Controller A    │
│ gets Repo #1    │               │ gets Repo #2    │
│                 │               │                 │
│ Service B       │               │ Service B       │
│ gets Repo #1    │               │ gets Repo #2    │
│ (same instance) │               │ (same instance) │
└─────────────────┘               └─────────────────┘
```
- **Use for**: Database contexts, repositories, unit of work
- **Why**: Each request gets fresh state, disposed at end of request

**Singleton Lifetime:**
```
Application Lifetime:
┌─────────────────────────────────────┐
│         Instance #1                  │
│  (shared by all requests/threads)   │
└─────────────────────────────────────┘
```
- **Use for**: Connection factories, caches, configuration
- **Why**: Expensive to create, stateless or thread-safe state

**Transient Lifetime:**
```
Request 1:                         Request 2:
┌─────────────────┐               ┌─────────────────┐
│ Service A       │               │ Service A       │
│ gets Handler #1 │               │ gets Handler #3 │
│                 │               │                 │
│ Service B       │               │ Service B       │
│ gets Handler #2 │               │ gets Handler #4 │
└─────────────────┘               └─────────────────┘
```
- **Use for**: Lightweight, stateless services, event handlers
- **Why**: No shared state, fresh instance each time

### 3. Complete Program.cs Breakdown

```csharp
var builder = WebApplication.CreateBuilder(args);

// ====== 1. DATABASE CONFIGURATION ======
builder.Services.AddDbContext<BankingDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));
// Creates scoped DbContext for each request

// ====== 2. CORE DOMAIN SERVICES (Scoped) ======
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
// Scoped because they use DbContext (which is scoped)

// ====== 3. DOMAIN EVENT DISPATCHER ======
builder.Services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
// Scoped to match DbContext lifetime

// ====== 4. MEDIATR CONFIGURATION ======
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(CreateAccountCommandHandler).Assembly));
// Automatically registers all handlers in the assembly

// ====== 5. PIPELINE BEHAVIORS (Transient) ======
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(DomainEventsBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehaviour<,>));
// Open generic registration - applies to all IRequest types

// ====== 6. EVENT HANDLERS (Transient) ======
builder.Services.AddTransient<INotificationHandler<AccountCreatedEvent>, AccountCreatedEventHandler>();
builder.Services.AddTransient<INotificationHandler<MoneyTransferedEvent>, MoneyTransferredEventHandler>();
builder.Services.AddTransient<INotificationHandler<MoneyTransferedEvent>, RealTimeNotificationEventHandler>();
// Multiple handlers for same event - all get called

// ====== 7. VALIDATION ======
builder.Services.AddValidatorsFromAssembly(typeof(CreateAccountCommandValidator).Assembly);
// Auto-registers all IValidator<T> implementations

// ====== 8. AUTOMAPPER ======
builder.Services.AddAutoMapper(cfg => { }, typeof(AccountProfile).Assembly);
// Registers mapping profiles

// ====== 9. SERVICE BUS (Singleton) ======
builder.Services.Configure<ServiceBusConfiguration>(
    builder.Configuration.GetSection("ServiceBus"));
// Binds configuration section to strongly-typed object

builder.Services.AddSingleton<IServiceBusClientFactory>(provider =>
{
    var config = provider.GetRequiredService<IOptions<ServiceBusConfiguration>>().Value;
    var logger = provider.GetRequiredService<ILogger<ServiceBusClientFactory>>();
    return new ServiceBusClientFactory(config.ConnectionString, logger);
});
// Factory pattern with DI - creates singleton instance

builder.Services.AddSingleton<IBankingServiceBusSender>(provider =>
{
    var config = provider.GetRequiredService<IOptions<ServiceBusConfiguration>>().Value;
    var logger = provider.GetRequiredService<ILogger<BankingServiceBusSender>>();
    return new BankingServiceBusSender(config.ConnectionString, logger);
});

// ====== 10. BACKGROUND SERVICES ======
builder.Services.AddHostedService<MessageProcessingService>();
builder.Services.AddHostedService<OutboxBackgroundService>();
builder.Services.AddHostedService<DeadLetterQueueMonitorService>();
// Starts automatically when app starts, runs in background

// ====== 11. HANGFIRE (Scoped services for jobs) ======
builder.Services.AddHangfire(config =>
    config.UseSqlServerStorage(
        builder.Configuration.GetConnectionString("HangfireConnection")));
builder.Services.AddHangfireServer();

builder.Services.AddScoped<IDailyStatementService, DailyStatementService>();
builder.Services.AddScoped<IInterestCalculationService, InterestCalculationService>();
builder.Services.AddScoped<IAccountMaintenanceService, AccountMaintenanceService>();
builder.Services.AddScoped<IJobInitializationService, JobInitializationService>();

// ====== 12. SIGNALR ======
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
    options.MaximumReceiveMessageSize = 64 * 1024; // 64KB
}).AddMessagePackProtocol();
// Adds real-time communication with MessagePack for efficiency

builder.Services.AddSingleton<ConnectionStateService>();
builder.Services.AddScoped<INotificationBroadcaster, NotificationBroadcaster>();

// ====== 13. OUTBOX PATTERN ======
builder.Services.AddScoped<IOutboxMessageProcessor, OutboxMessageProcessor>();
// Processes outbox messages to publish events

// ====== 14. RESILIENCE (HTTP Client) ======
builder.Services.AddHttpClient<ICreditScoringServiceClient, CreditScoringServiceClient>();
builder.Services.AddSingleton<IResilientHttpClientService, ResilientHttpClientService>();
builder.Services.AddScoped<IResilienceService, ResilienceService>();
// Adds retry policies, circuit breakers

// ====== 15. GRPC ======
builder.Services.AddGrpc(options =>
{
    options.EnableDetailedErrors = true;
});
builder.Services.AddGrpcReflection();
// High-performance RPC framework

// ====== 16. CONTROLLERS ======
builder.Services.AddControllers();
// Adds MVC controller support

// ====== BUILD THE APP ======
var app = builder.Build();

// ====== MIDDLEWARE PIPELINE ======
// Order matters! Requests flow top to bottom

app.UseHttpsRedirection();
// Redirect HTTP to HTTPS

app.UseStaticFiles();
// Serve static files (wwwroot)

app.UseRouting();
// Enable routing

app.UseAuthorization();
// Check authorization (if configured)

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
// Custom exception handling

// ====== ENDPOINTS ======
app.MapControllers();
// Map REST API controllers

app.MapGrpcService<AccountGrpcService>();
// Map gRPC service

app.MapHub<NotificationHub>("/hubs/notifications");
app.MapHub<TransactionHub>("/hubs/transactions");
// Map SignalR hubs

// ====== STARTUP INITIALIZATION ======
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    // Initialize Hangfire jobs
    var jobInitService = services.GetRequiredService<IJobInitializationService>();
    await jobInitService.InitializeRecurringJobsAsync();

    // Ensure Service Bus infrastructure exists
    var sbAdmin = services.GetRequiredService<IServiceBusAdministration>();
    await sbAdmin.EnsureInfrastructureExistsAsync();
}

// ====== RUN THE APP ======
app.Run();
```

### 4. Routing in ASP.NET Core

**Attribute Routing:**
```csharp
[ApiController]
[Route("api/[controller]")]  // Base route: /api/accounts
public class AccountsController : ControllerBase
{
    [HttpGet("{accountNumber}")]  // GET /api/accounts/{accountNumber}
    public async Task<ActionResult> GetAccountDetails(string accountNumber)

    [HttpPost]                     // POST /api/accounts
    public async Task<ActionResult> CreateAccount([FromBody] CreateAccountRequest request)

    [HttpPost("{accountNumber}/transfer")]  // POST /api/accounts/{accountNumber}/transfer
    public async Task<ActionResult> TransferMoney(string accountNumber, ...)

    [HttpGet("{accountNumber}/transactions")]  // GET /api/accounts/{accountNumber}/transactions
    public async Task<ActionResult> GetTransactionHistory(string accountNumber)
}
```

**Route Tokens:**
- `[controller]` - Replaced with controller name (minus "Controller" suffix)
- `{accountNumber}` - Route parameter (captured from URL)
- `[FromBody]` - Deserialize request body to object
- `[FromQuery]` - Get from query string (?key=value)

**HTTP Methods:**
- `GET` - Retrieve data (safe, idempotent)
- `POST` - Create new resource
- `PUT` - Update entire resource
- `PATCH` - Partial update
- `DELETE` - Remove resource

### 5. Configuration Sources

```csharp
// appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=CoreBanking;",
    "HangfireConnection": "Server=...;Database=Hangfire;"
  },
  "ServiceBus": {
    "ConnectionString": "Endpoint=sb://...",
    "Topics": { ... }
  }
}

// Access in code:
builder.Configuration.GetConnectionString("DefaultConnection");
builder.Configuration.GetSection("ServiceBus").Get<ServiceBusConfiguration>();
builder.Configuration["SomeKey"];
```

**Configuration Priority (highest to lowest):**
1. Command-line arguments
2. Environment variables
3. User secrets (development only)
4. appsettings.{Environment}.json
5. appsettings.json

---

## Design Patterns Explained

### 1. MediatR Pattern

**What is it?**
MediatR is an implementation of the Mediator pattern - it decouples the sender of a request from its handler.

**Without MediatR:**
```csharp
public class AccountsController
{
    private readonly CreateAccountCommandHandler _createHandler;
    private readonly GetAccountDetailsQueryHandler _getHandler;
    // ... more handlers

    public async Task<ActionResult> CreateAccount(...)
    {
        var result = await _createHandler.Handle(command, token);
    }
}
```
Controller knows about all handlers - tight coupling!

**With MediatR:**
```csharp
public class AccountsController
{
    private readonly IMediator _mediator;

    public async Task<ActionResult> CreateAccount(...)
    {
        var result = await _mediator.Send(command);
    }
}
```
Controller only knows about IMediator - loose coupling!

**MediatR Flow:**
```
Command → IMediator.Send() →
  Pipeline Behaviors (Validation, Logging, etc.) →
    Handler → Response
```

### 2. CQRS (Command Query Responsibility Segregation)

**What is it?**
Separate the read model (queries) from the write model (commands).

**Commands (Write):**
- Change state
- Return success/failure
- Validate business rules
- Go through domain model

**Queries (Read):**
- Don't change state
- Return data
- Can bypass domain model
- Optimize for read performance

**Example in your code:**
```
Commands:                          Queries:
CreateAccountCommand      →        GetAccountDetailsQuery
TransferMoneyCommand      →        GetAccountSummaryQuery
CreateCustomerCommand     →        GetTransactionHistoryQuery
```

**Why CQRS?**
1. **Separation of concerns** - Read/write logic separated
2. **Scalability** - Scale reads and writes independently
3. **Performance** - Optimize each path separately
4. **Simplicity** - Each handler does one thing

### 3. Domain Events

**What is it?**
A record of something that happened in the domain.

**Pattern:**
```
Entity does something → Raises event → Handlers react
```

**Your Implementation:**
```csharp
// In Account.Create():
AddDomainEvent(new AccountCreatedEvent(...));

// In DomainEventDispatcher:
foreach (var domainEvent in entity.DomainEvents)
{
    await _publisher.Publish(domainEvent);
}

// Handlers:
AccountCreatedEventHandler → Log
RealTimeNotificationEventHandler → SignalR broadcast
OutboxMessageProcessor → Service Bus publish
```

**Why Domain Events?**
1. **Decoupling** - Entity doesn't know about handlers
2. **Extensibility** - Add handlers without changing entity
3. **Audit trail** - Record of what happened
4. **Integration** - Foundation for event-driven architecture

### 4. Outbox Pattern

**Problem:**
You need to:
1. Save data to database
2. Publish event to message queue

What if step 2 fails? Data is saved but event is lost!

**Solution: Outbox Pattern**
```
1. Save data to database
2. Save event to OutboxMessages table (same transaction)
3. Background service reads OutboxMessages
4. Publishes to message queue
5. Marks as processed
```

**Why?**
- **Reliability** - Events never lost
- **Consistency** - Event saved with data atomically
- **Retry** - Failed publishes retry automatically

### 5. Repository Pattern

**What is it?**
An abstraction over data access.

**Without Repository:**
```csharp
public class CreateAccountCommandHandler
{
    private readonly BankingDbContext _context;

    public async Task Handle(...)
    {
        var customer = await _context.Customers.FindAsync(id);
        await _context.Accounts.AddAsync(account);
        await _context.SaveChangesAsync();
    }
}
```
Handler knows about EF Core - coupled to infrastructure!

**With Repository:**
```csharp
public class CreateAccountCommandHandler
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;

    public async Task Handle(...)
    {
        var customer = await _customerRepository.GetByIdAsync(id);
        await _accountRepository.AddAsync(account);
        await _unitOfWork.SaveChangesAsync();
    }
}
```
Handler only knows about abstractions!

**Why Repository?**
1. **Abstraction** - Hide data access details
2. **Testability** - Mock repositories in tests
3. **Query encapsulation** - Complex queries hidden
4. **Single responsibility** - Repository only handles data

---

## Request Flow Examples

### Example 1: Create Account (Complete Flow)

```
1. Client sends HTTP POST to /api/accounts
   Body: { customerId: "abc-123", accountType: "Savings", initialDeposit: 5000 }

2. ASP.NET Core Routing matches to AccountsController.CreateAccount
   ┌─────────────────────────────┐
   │ [HttpPost]                  │
   │ CreateAccount(request)      │
   └─────────────────────────────┘

3. Controller maps request to command
   var command = _mapper.Map<CreateAccountCommand>(request);
   // CreateAccountCommand(CustomerId: abc-123, AccountType: Savings, InitialDeposit: 5000)

4. Controller sends to MediatR
   var result = await _mediator.Send(command);

5. MediatR Pipeline Behaviors execute in order:

   5.1 DomainEventsBehavior.Handle()
       │ Logs: "Processing domain events for CreateAccountCommand"
       │ Calls next()
       │ ↓

   5.2 ValidationBehavior.Handle()
       │ Gets CreateAccountCommandValidator
       │ Validates:
       │   - CustomerId not empty ✓
       │   - AccountType valid ✓
       │   - InitialDeposit > 0 ✓
       │   - InitialDeposit <= 1,000,000 ✓
       │ Calls next()
       │ ↓

   5.3 LoggingBehavior.Handle()
       │ Logs: "Handling command CreateAccountCommand with payload {...}"
       │ Starts Stopwatch
       │ Calls next()
       │ ↓

6. CreateAccountCommandHandler.Handle() executes:

   6.1 Validate customer exists:
       var customer = await _customerRepository.GetByIdAsync(customerId);
       if (customer is null) return Result.Failure("Customer not found");

   6.2 Generate unique account number:
       var accountNumber = await GenerateUniqueAccountNumberAsync();
       // "1234567890"

   6.3 Create account via domain factory:
       var account = Account.Create(customerId, accountNumber, Savings, Money(5000, "NGN"));

       Inside Account.Create():
       │ Validate initialBalance >= 0 ✓
       │ Validate initialBalance <= 1,000,000 ✓
       │ Create Account instance
       │ AddDomainEvent(new AccountCreatedEvent(...))
       │ Return account

   6.4 Persist to database:
       await _accountRepository.AddAsync(account);
       // EF Core marks as Added

       await _unitOfWork.SaveChangesAsync();
       // EF Core generates SQL:
       // INSERT INTO Accounts (...) VALUES (...)
       // INSERT INTO OutboxMessages (...) VALUES (...)
       // Executes in single transaction

   6.5 Return success:
       return Result<Guid>.Success(account.Id.Value);

7. Pipeline unwinds:

   7.1 LoggingBehavior completes:
       │ Stops Stopwatch
       │ Logs: "Command CreateAccountCommand handled in 150ms"
       │ Returns result
       │ ↑

   7.2 ValidationBehavior completes:
       │ Returns result
       │ ↑

   7.3 DomainEventsBehavior completes:
       │ Calls _dispatcher.DispatchDomainEventsAsync()
       │
       │ Inside DomainEventDispatcher:
       │ │ Gets Account from EF ChangeTracker
       │ │ Gets DomainEvents: [AccountCreatedEvent]
       │ │ For each event:
       │ │   await _publisher.Publish(event);
       │ │
       │ │   Publishes to all INotificationHandler<AccountCreatedEvent>:
       │ │   │ AccountCreatedEventHandler.Handle()
       │ │   │   Logs: "Account {AccountNumber} created"
       │ │   │
       │ │   │ RealTimeNotificationEventHandler.Handle()
       │ │   │   Broadcasts via SignalR to subscribed clients
       │ │
       │ │ Clears events from Account
       │ │
       │ Returns result
       │ ↑

8. Controller receives result:
   if (!result.IsSuccess)
       return BadRequest(...);

   return CreatedAtAction(..., ApiResponse.CreateSuccess(result.Value));
   // HTTP 201 Created
   // Location: /api/accounts/1234567890
   // Body: { success: true, data: "guid-of-account" }

9. Background Processing (async):

   OutboxBackgroundService runs every 10 seconds:
   │ Queries: SELECT * FROM OutboxMessages WHERE ProcessedOn IS NULL
   │ Gets AccountCreatedEvent message
   │ Deserializes to AccountCreatedEvent
   │ Calls ServiceBusEventPublisher.PublishAsync()
   │ │ Serializes event to JSON
   │ │ Adds properties (EventType, EventId, etc.)
   │ │ Sends to "account-events" topic
   │ Marks message.ProcessedOn = now
   │ Saves changes

   Azure Service Bus subscriptions receive event:
   │ notifications subscription → Could send welcome email
   │ analytics subscription → Update reports
   │ fraud-detection subscription → Check for suspicious activity
```

### Example 2: Transfer Money (With Error Handling)

```
1. HTTP POST /api/accounts/1234567890/transfer
   Body: { destinationAccountNumber: "0987654321", amount: 10000, currency: "NGN" }

2. AccountsController.TransferMoney():
   var command = new TransferMoneyCommand(
       "1234567890", "0987654321", 10000, "NGN");
   var result = await _mediator.Send(command);

3. Pipeline executes (validation, logging)...

4. TransferMoneyCommandHandler.Handle():

   4.1 Get source account:
       var source = await _accountRepository.GetByAccountNumberAsync("1234567890");
       // Account: Balance = 5000 NGN

   4.2 Get destination account:
       var destination = await _accountRepository.GetByAccountNumberAsync("0987654321");
       // Account: Balance = 2000 NGN

   4.3 Execute transfer:
       var result = source.Transfer(Money(10000, "NGN"), destination, ref, desc);

       Inside Account.Transfer():
       │ Check source.IsActive ✓
       │ Check destination.IsActive ✓
       │ Check source.Balance >= amount ✗
       │   5000 < 10000
       │
       │ AddDomainEvent(new InsufficientFundEvent(
       │     "1234567890", 10000, 5000, "Insufficient funds"));
       │
       │ Return Result.Failure("Insufficient funds");

   4.4 Handler returns:
       return Result.Failure("Insufficient funds");

5. Pipeline unwinds:

   DomainEventsBehavior:
   │ Dispatches InsufficientFundEvent
   │ │ InsufficientFundsEventHandler.Handle()
   │ │   Logs warning
   │ │   Could notify fraud detection
   │ │   Could send alert to customer

6. Controller receives failure:
   if (!result.IsSuccess)
       return BadRequest(ApiResponse.CreateFailure("Insufficient funds"));
   // HTTP 400 Bad Request
   // Body: { success: false, error: "Insufficient funds" }

7. No OutboxMessage created (transaction rolled back)
```

---

## Mind Maps

### Mind Map 1: Event-Driven Design

```
                              EVENT-DRIVEN DESIGN
                                      │
                    ┌─────────────────┼─────────────────┐
                    │                 │                 │
              ┌─────▼─────┐    ┌─────▼─────┐    ┌─────▼─────┐
              │  EVENTS   │    │ PRODUCERS │    │ CONSUMERS │
              └─────┬─────┘    └─────┬─────┘    └─────┬─────┘
                    │                │                 │
        ┌───────────┼───────────┐    │     ┌───────────┼───────────┐
        │           │           │    │     │           │           │
   ┌────▼────┐ ┌────▼────┐ ┌────▼────┐    ┌────▼────┐ ┌────▼────┐
   │ DOMAIN  │ │INTEGRA- │ │  EVENT  │    │  EVENT  │ │ SAGA /  │
   │ EVENTS  │ │  TION   │ │  STORE  │    │ HANDLER │ │  PROCESS│
   └────┬────┘ │ EVENTS  │ └─────────┘    └────┬────┘ │  MANAGER│
        │      └────┬────┘                      │      └─────────┘
        │           │                           │
   ┌────▼────────────▼────┐              ┌─────▼─────┐
   │ Your Code Examples:  │              │   TYPES:  │
   │                      │              │           │
   │ AccountCreatedEvent  │              │ • Sync    │
   │ MoneyTransferedEvent │              │ • Async   │
   │ InsufficientFundEvent│              │ • Pub/Sub │
   └──────────────────────┘              │ • Queue   │
                                         └───────────┘

                    KEY CONCEPTS
                         │
        ┌────────────────┼────────────────┐
        │                │                │
   ┌────▼────┐     ┌────▼────┐     ┌────▼────┐
   │DECOUPLING│    │RELIABILITY│   │SCALABILITY│
   └────┬────┘     └────┬────┘     └────┬────┘
        │                │                │
   Producers      Outbox Pattern    Multiple
   don't know     guarantees        consumers
   consumers      delivery          process in
                                    parallel

                    PATTERNS
                        │
        ┌───────────────┼───────────────┐
        │               │               │
   ┌────▼────┐    ┌────▼────┐    ┌────▼────┐
   │  OUTBOX │    │DEAD LETTER│   │  RETRY  │
   │ PATTERN │    │   QUEUE   │   │  POLICY │
   └────┬────┘    └────┬────┘    └────┬────┘
        │               │               │
   Save event      Failed          Exponential
   with data       messages        backoff
   atomically      quarantined     with jitter
```

### Mind Map 2: Domain-Driven Design (DDD)

```
                          DOMAIN-DRIVEN DESIGN
                                   │
                    ┌──────────────┼──────────────┐
                    │              │              │
              ┌─────▼─────┐  ┌─────▼─────┐  ┌─────▼─────┐
              │  TACTICAL │  │ STRATEGIC │  │  BUILDING │
              │  PATTERNS │  │  PATTERNS │  │   BLOCKS  │
              └─────┬─────┘  └─────┬─────┘  └─────┬─────┘
                    │              │              │
    ┌───────────────┼───────┐     │     ┌────────┼────────┐
    │               │       │     │     │        │        │
┌───▼───┐      ┌───▼───┐   │     │ ┌───▼───┐ ┌──▼───┐ ┌──▼───┐
│ENTITY │      │ VALUE │   │     │ │BOUNDED│ │UBIQUI-│ │CONTEXT│
│       │      │ OBJECT│   │     │ │CONTEXT│ │TOUS   │ │  MAP  │
│Account│      │ Money │   │     │ │       │ │LANGUAGE│└───────┘
│Customer       │AccountId│ │     │ │Banking│ │       │
│Transaction    └───┬───┘   │     │ │Domain │ │Account│
└───┬───┘          │       │     │ │       │ │Transfer│
    │              │       │     │ └───────┘ │Balance │
    │      Immutable      │     │           └────────┘
    │      Self-validating│     │
    │      Equality by    │     │
    │      value          │     │

    ┌───▼───┐      ┌───▼───┐
    │AGGREGATE│    │ DOMAIN │
    │  ROOT   │    │ EVENT  │
    │         │    │        │
    │ Account │    │AccountCreatedEvent
    │ (root)  │    │MoneyTransferedEvent
    │  ├─Transactions     │
    │  └─Balance          │
    └───┬───┘      └───┬───┘
        │              │
    Consistency    Immutable
    Boundary       Past Tense
    Transaction    Records what
    Boundary       happened

                    LAYERS
                      │
        ┌─────────────┼─────────────┐
        │             │             │
   ┌────▼────┐  ┌────▼────┐  ┌────▼────┐
   │  DOMAIN │  │APPLICATION│ │INFRASTR-│
   │  LAYER  │  │  LAYER    │ │ UCTURE  │
   └────┬────┘  └────┬────┘  └────┬────┘
        │            │            │
   Entities     Use Cases    Repositories
   Value Obj    Handlers     DbContext
   Events       Behaviors    Service Bus
   Rules        DTOs         External APIs

        YOUR CODE STRUCTURE:
        ├── CoreBanking.Core (Domain)
        ├── CoreBanking.APP (Application)
        ├── CoreBanking.Infrastructure
        └── CoreBanking.API (Presentation)
```

### Mind Map 3: Microservices Architecture

```
                           MICROSERVICES
                                │
                ┌───────────────┼───────────────┐
                │               │               │
          ┌─────▼─────┐   ┌─────▼─────┐   ┌─────▼─────┐
          │   CORE    │   │COMMUNICATION│  │  PATTERNS │
          │ PRINCIPLES│   │  PATTERNS   │  │           │
          └─────┬─────┘   └─────┬─────┘   └─────┬─────┘
                │               │               │
    ┌───────────┼───────┐      │     ┌──────────┼──────────┐
    │           │       │      │     │          │          │
┌───▼───┐  ┌───▼───┐ ┌──▼──┐  │  ┌──▼───┐  ┌───▼───┐  ┌──▼───┐
│ SINGLE │  │BOUNDED│ │LOOSE│  │  │  CQRS │  │ EVENT  │  │SAGA  │
│RESPONSI│  │CONTEXT│ │COUPL│  │  │       │  │SOURCING│  │      │
│ BILITY │  │       │ │ING  │  │  │Separate│ │Store   │  │Long  │
└───┬───┘  └───┬───┘ └──┬──┘  │  │Read/   │ │all     │  │running│
    │          │        │     │  │Write   │ │events  │  │transac│
 One service   Own    Services   └────────┘ └────────┘  │tions  │
 does one      database don't                            └───────┘
 thing         per     share
               service  code

                    ┌────▼────┐
                    │COMMUNICATION│
                    └────┬────┘
                         │
        ┌────────────────┼────────────────┐
        │                │                │
   ┌────▼────┐     ┌────▼────┐     ┌────▼────┐
   │   SYNC  │     │  ASYNC  │     │  EVENT  │
   │  (REST) │     │ (QUEUE) │     │   BUS   │
   └────┬────┘     └────┬────┘     └────┬────┘
        │                │                │
   HTTP/gRPC        Message         Pub/Sub
   Request/         Queues          Topics
   Response         (RabbitMQ)      (Kafka)
                    Service Bus     Event Grid

        YOUR CODE:
        REST API (Controllers)
        gRPC Services
        Azure Service Bus
        SignalR (Real-time)

                    DATA MANAGEMENT
                          │
            ┌─────────────┼─────────────┐
            │             │             │
       ┌────▼────┐  ┌────▼────┐  ┌────▼────┐
       │ DATABASE │  │  OUTBOX │  │ EVENTUAL │
       │PER SERVICE│ │ PATTERN │  │CONSISTENCY
       └────┬────┘  └────┬────┘  └────┬────┘
            │            │            │
       Each service  Reliable    Data may be
       owns its      event       temporarily
       data          publishing  inconsistent

        YOUR CODE:
        BankingDbContext (SQL Server)
        OutboxMessages table
        Domain Events for sync
        Service Bus for async

                    INFRASTRUCTURE
                          │
            ┌─────────────┼─────────────┐
            │             │             │
       ┌────▼────┐  ┌────▼────┐  ┌────▼────┐
       │  SERVICE │  │  CIRCUIT │  │  RETRY  │
       │ DISCOVERY│  │  BREAKER │  │  POLICY │
       └─────────┘  └────┬────┘  └────┬────┘
                         │            │
                    Fail fast    Exponential
                    when service backoff
                    is down

        YOUR CODE:
        Polly Policies
        Resilient HTTP Client
        Circuit Breaker
```

### Mind Map 4: Your CoreBanking Architecture

```
                      COREBANKING SYSTEM
                             │
            ┌────────────────┼────────────────┐
            │                │                │
      ┌─────▼─────┐    ┌─────▼─────┐    ┌─────▼─────┐
      │   ENTRY   │    │  BUSINESS │    │   DATA    │
      │   POINTS  │    │   LOGIC   │    │   LAYER   │
      └─────┬─────┘    └─────┬─────┘    └─────┬─────┘
            │                │                │
    ┌───────┼───────┐       │         ┌──────┼──────┐
    │       │       │       │         │      │      │
┌───▼───┐┌──▼──┐┌───▼───┐   │     ┌───▼──┐┌──▼──┐┌──▼───┐
│  REST ││gRPC ││SignalR│   │     │  EF  ││Azure ││Hang- │
│  API  ││     ││ Hubs  │   │     │ Core ││Service│  fire│
│       ││     ││       │   │     │      ││ Bus  ││      │
│Control││Account       │   │     │DbCntx││Topics││Jobs  │
│ lers  ││GrpcSvc       │   │     │Repos ││Queue ││Scheds│
└───┬───┘└──┬──┘└───┬───┘   │     └──┬───┘└──┬──┘└──┬───┘
    │       │       │       │        │       │      │
    └───────┴───────┴───────┼────────┴───────┴──────┘
                            │
                    ┌───────▼───────┐
                    │   APPLICATION │
                    │     LAYER     │
                    └───────┬───────┘
                            │
        ┌───────────────────┼───────────────────┐
        │                   │                   │
   ┌────▼────┐        ┌────▼────┐        ┌────▼────┐
   │ COMMANDS│        │ QUERIES │        │BEHAVIORS│
   │         │        │         │        │         │
   │Create   │        │Get      │        │Logging  │
   │Account  │        │Account  │        │Validatn │
   │Transfer │        │Details  │        │Domain   │
   │Money    │        │Get      │        │Events   │
   └────┬────┘        │Transactn│        └────┬────┘
        │             │History  │              │
        │             └────┬────┘              │
        │                  │                   │
        └──────────────────┼───────────────────┘
                           │
                    ┌──────▼──────┐
                    │   DOMAIN    │
                    │    LAYER    │
                    └──────┬──────┘
                           │
        ┌──────────────────┼──────────────────┐
        │                  │                  │
   ┌────▼────┐       ┌────▼────┐       ┌────▼────┐
   │ENTITIES │       │  VALUE  │       │ DOMAIN  │
   │         │       │ OBJECTS │       │ EVENTS  │
   │Account  │       │ Money   │       │Account  │
   │Customer │       │AccountId│       │Created  │
   │Transaction      │AccountNo│       │Money    │
   └─────────┘       └─────────┘       │Transferd│
                                       └─────────┘

                    EVENT FLOW
                        │
        ┌───────────────┼───────────────┐
        │               │               │
   ┌────▼────┐    ┌────▼────┐    ┌────▼────┐
   │ DOMAIN  │    │ OUTBOX  │    │ SERVICE │
   │ EVENT   │    │ MESSAGE │    │   BUS   │
   │DISPATCH │    │  TABLE  │    │  TOPIC  │
   └────┬────┘    └────┬────┘    └────┬────┘
        │              │               │
   In-process     Persisted      External
   immediate      reliable       async
   handlers       delivery       subscribers

   Handlers:                   Subscriptions:
   - Log event                 - notifications
   - SignalR broadcast         - analytics
   - Update cache              - fraud-detection
                               - reporting
```

---

## Summary

### What You've Built

A **production-grade banking system** that demonstrates:

1. **Clean Architecture** - Clear separation of concerns
2. **Domain-Driven Design** - Rich domain model with business rules
3. **CQRS** - Optimized read and write paths
4. **Event-Driven** - Decoupled, scalable system
5. **Reliable Messaging** - Outbox pattern guarantees delivery
6. **Real-Time Updates** - SignalR for instant notifications
7. **Background Processing** - Hangfire for scheduled jobs
8. **High Performance** - gRPC alongside REST
9. **Resilience** - Retry policies, circuit breakers
10. **Type Safety** - Value objects prevent bugs

### Key Takeaways

1. **Dependency Injection** is the foundation - loose coupling, testability
2. **Service Lifetimes** matter - Scoped for request state, Singleton for shared, Transient for stateless
3. **Pipeline Behaviors** handle cross-cutting concerns without repetition
4. **Domain Events** decouple what happened from who cares
5. **Outbox Pattern** ensures reliable event publishing
6. **Repository Pattern** abstracts data access
7. **CQRS** separates reads from writes for better scalability

This architecture is used by major companies like:
- **Netflix** - Event-driven microservices
- **Amazon** - CQRS for scalability
- **Uber** - Domain-driven design
- **Banks** - Outbox pattern for consistency

You now have a solid foundation for building enterprise-grade applications!

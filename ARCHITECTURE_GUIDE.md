# 🏦 CoreBanking Architecture Guide
## Understanding DDD, CQRS, and MediatR from Node.js Perspective

---

## 📚 **PART 1: THE BIG PICTURE**

### **What is Clean Architecture / DDD?**

Think of it like **layers of an onion** - each layer only knows about the layers inside it, never the layers outside.

```
┌─────────────────────────────────────────┐
│   PRESENTATION LAYER (API/Controllers)  │  ← Entry point (like Express routes)
├─────────────────────────────────────────┤
│   APPLICATION LAYER (Commands/Queries)  │  ← Business use cases (like service layer)
├─────────────────────────────────────────┤
│   DOMAIN LAYER (Entities/Business Logic)│  ← Core business rules (pure logic)
├─────────────────────────────────────────┤
│   INFRASTRUCTURE (Database/External)    │  ← Data access (like Prisma/Mongoose)
└─────────────────────────────────────────┘
```

**Why layers?**
- **Separation of Concerns**: Each layer has ONE job
- **Testability**: Test business logic without database
- **Maintainability**: Change database without changing business rules
- **Independence**: Business logic doesn't depend on frameworks

---

### **What is CQRS? (Command Query Responsibility Segregation)**

**Simple Explanation**: Separate **WRITING** data from **READING** data.

- **Commands** = Change something (Create, Update, Delete, Transfer)
- **Queries** = Read something (Get Account, List Transactions)

**Why?**
- Different optimization strategies
- Clearer intent ("I want to CREATE" vs "I want to READ")
- Easier to scale reads and writes independently
- Better separation of concerns

**Node.js Analogy:**
```javascript
// Instead of one service method:
async function createAccount(data) { ... }  // Command
async function getAccount(id) { ... }        // Query

// CQRS separates them:
Commands.CreateAccountCommand
Queries.GetAccountQuery
```

---

### **What is MediatR?**

**MediatR = Message Bus / Middleware Pipeline** (like Express middleware but for business logic)

**What it does:**
1. Receives a "request" (Command or Query)
2. Routes it to the correct "handler"
3. Executes "behaviors" (like middleware) before/after
4. Returns the result

**Node.js Analogy:**
```javascript
// Without MediatR (direct):
controller → repository → database

// With MediatR (message bus):
controller → MediatR → [Behaviors] → Handler → Repository → Database
                      └─ ValidationBehavior (like middleware)
                      └─ LoggingBehavior (like middleware)
```

**Benefits:**
- **Decoupling**: Controller doesn't know about handlers
- **Pipeline**: Add behaviors (logging, validation, caching) easily
- **Single Responsibility**: Each handler does ONE thing

---

## 🔄 **PART 2: REQUEST FLOW (Step-by-Step)**

### **Scenario: User Creates a New Account**

Let's trace a complete request from HTTP to Database and back:

```
HTTP POST /api/accounts/create
{
  "customerId": "123...",
  "accountType": "Savings",
  "initialDeposit": 5000,
  "currency": "NGN"
}
```

---

### **STEP 1: Entry Point - Program.cs**

**File**: `CoreBanking.API/Program.cs`

**What it does**: This is like your `app.js` or `server.js` in Node.js - it's the **application startup**.

```csharp
public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Register services (like dependency injection container)
        builder.Services.AddControllers();          // Enable controllers
        builder.Services.AddDbContext<...>();      // Database connection
        builder.Services.AddScoped<...>();        // Register repositories

        var app = builder.Build();
        app.MapControllers();                     // Map routes to controllers
        app.Run();                                 // Start server
    }
}
```

**Why we need it:**
- Configures the entire application
- Sets up dependency injection (like a service registry)
- Registers all services (repositories, handlers, etc.)
- Configures middleware pipeline

**Node.js Equivalent:**
```javascript
// Express.js
const app = express();
app.use(express.json());
app.use('/api', routes);
app.listen(3000);
```

**Current State**: Missing MediatR and UnitOfWork registration!

---

### **STEP 2: Routing - Controllers**

**File**: `CoreBanking.API/Controllers/AccountController.cs`

**What it does**: Receives HTTP requests and returns HTTP responses (like Express route handlers).

**Why Controllers?**
- **HTTP-specific**: They understand HTTP (GET, POST, status codes)
- **Thin layer**: Should NOT contain business logic
- **Entry point**: First place HTTP requests land

**Current Implementation (NOT using CQRS):**
```csharp
[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly IAccountRepository _accountRepository;  // ❌ Direct access

    [HttpGet]
    public async Task<IActionResult> GetAllAccounts()
    {
        var accounts = await _accountRepository.GetAllAsync();  // ❌ Direct call
        return Ok(accounts);
    }
}
```

**Problem**: Controller directly talks to repository (skips application layer)

**Ideal Implementation (Using CQRS + MediatR):**
```csharp
[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly IMediator _mediator;  // ✅ Message bus

    [HttpPost("create")]
    public async Task<IActionResult> CreateAccount(CreateAccountCommand command)
    {
        var result = await _mediator.Send(command);  // ✅ Send to MediatR
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Errors);
    }
}
```

**Flow**: HTTP Request → Controller → **Sends to MediatR** → (we'll see what happens next)

---

### **STEP 3: MediatR - Message Bus**

**What happens**: MediatR receives the Command/Query and routes it through a pipeline.

**Pipeline Flow:**
```
Command/Query enters
    ↓
[ValidationBehavior]  ← Validates the request (like middleware)
    ↓
[LoggingBehavior]    ← Logs the request (like middleware)
    ↓
[Handler]            ← The actual business logic handler
    ↓
Result returns
```

**Why Behaviors?**
- **Cross-cutting concerns**: Validation, logging, caching, authorization
- **Reusable**: Apply to ALL requests automatically
- **Separation**: Keep handlers focused on business logic

**Node.js Middleware Analogy:**
```javascript
// Express middleware
app.use(validateRequest);   // Like ValidationBehavior
app.use(logRequest);        // Like LoggingBehavior
app.use('/api', routes);    // Actual handler
```

---

### **STEP 4: Application Layer - Command Handler**

**File**: `CoreBanking.APP/Accounts/Commands/CreatedAccount/CreateAccountCommand.cs`

**What it does**: Contains the **use case** (the business workflow for creating an account).

**Two parts:**

#### **A. Command (The Request)**
```csharp
public record CreateAccountCommand : Icommand<Guid>
{
    public CustomerId CustomerId { get; init; }
    public string AccountType { get; init; }
    public decimal InitialDeposit { get; init; }
    public string Currency { get; init; } = "NGN";
}
```
**Why**: This is the **data structure** for the request. Think of it like a DTO (Data Transfer Object).

#### **B. Handler (The Use Case)**
```csharp
public class CreateAccountCommandHandler : IRequestHandler<CreateAccountCommand, Result<Guid>>
{
    private readonly IAccountRepository _accountRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public async Task<Result<Guid>> Handle(CreateAccountCommand request, ...)
    {
        // 1. Validate customer exists
        // 2. Generate unique account number
        // 3. Create account using domain factory
        // 4. Save to database
        // 5. Return result
    }
}
```

**Why Handler?**
- **Single Responsibility**: ONE use case per handler
- **Orchestration**: Coordinates multiple domain operations
- **Application Logic**: Workflow logic (not business rules)

**Node.js Analogy:**
```javascript
// Service layer
async function createAccount(data) {
    // 1. Validate customer
    // 2. Generate account number
    // 3. Create account
    // 4. Save to database
    // 5. Return result
}
```

---

### **STEP 5: Domain Layer - Entity Factory**

**File**: `CoreBanking.Core/Entities/Account.cs`

**What happens**: Handler calls `Account.Create()` - this is a **domain factory method**.

```csharp
public static Account Create(
    CustomerId customerId,
    AccountNumber accountNumber,
    AccountType accountType,
    Money initialBalance)
{
    // Domain validation (business rules)
    if (initialBalance.Amount < 0)
        throw new InvalidOperationException("Initial balance cannot be negative");

    if (initialBalance.Amount > 1000000)
        throw new InvalidOperationException("Initial deposit too large");

    // Create account using private constructor
    var account = new Account(...);

    // Raise domain event
    account.AddDomainEvent(new AccountCreatedEvent(account));

    return account;
}
```

**Why Domain Factory?**
- **Encapsulation**: Business rules live in the domain
- **Invariants**: Ensures account is ALWAYS created correctly
- **Domain Events**: Can notify other parts of system

**Key Point**: Business rules (like "initial balance cannot be negative") belong HERE, not in handlers or controllers!

---

### **STEP 6: Domain Layer - Entity State**

**What happens**: Account entity is created with business rules enforced.

```csharp
public class Account : ISoftDelete
{
    // Properties (encapsulated - private setters)
    public AccountId AccountId { get; private set; }
    public Money Balance { get; private set; }

    // Business operations (public API)
    public Transaction Deposit(Money amount, string description) { ... }
    public Transaction Withdraw(Money amount, ...) { ... }
    public void Transfer(Money amount, Account destination, ...) { ... }

    // Business rules enforced here:
    // - Cannot deposit to inactive account
    // - Cannot withdraw more than balance
    // - Savings account limited to 6 withdrawals
}
```

**Why Private Setters?**
- **Encapsulation**: Cannot modify directly, must use methods
- **Invariants**: Business rules are ALWAYS enforced
- **Domain Logic**: Business rules live in entities

**Node.js Analogy:**
```javascript
// Class with private fields and methods
class Account {
    #balance = 0;  // Private

    deposit(amount) {
        if (amount <= 0) throw new Error("Amount must be positive");
        this.#balance += amount;  // Business rule enforced
    }
}
```

---

### **STEP 7: Infrastructure - Repository**

**File**: `CoreBanking.Infrastructure/Repositories/AccountRepository.cs`

**What it does**: Handles **data access** (like a database client).

```csharp
public class AccountRepository : IAccountRepository
{
    private readonly BankingDbContext _context;  // EF Core context

    public async Task AddAsync(Account account)
    {
        await _context.Accounts.AddAsync(account);  // Add to change tracker
    }

    // Note: Doesn't save yet! Just adds to change tracker
}
```

**Why Repository?**
- **Abstraction**: Hides database details from domain
- **Testability**: Can mock repository for testing
- **Flexibility**: Could switch from SQL Server to MongoDB easily

**Node.js Analogy:**
```javascript
// Database abstraction
class AccountRepository {
    async add(account) {
        return await db.accounts.insert(account);  // Prisma/Mongoose
    }
}
```

---

### **STEP 8: Infrastructure - Unit of Work**

**File**: `CoreBanking.Infrastructure/Data/UnitOfWork.cs`

**What it does**: **Transaction management** - ensures all changes are saved together atomically.

```csharp
public class UnitOfWork : IUnitOfWork
{
    private readonly BankingDbContext _context;

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        // This is where database transaction happens
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
```

**Why Unit of Work?**
- **Atomicity**: All changes save together or none at all
- **Transaction boundary**: Defines where transaction starts/ends
- **Consistency**: If one save fails, all fails

**Node.js Analogy:**
```javascript
// Transaction management
async function saveChanges() {
    await db.transaction(async (trx) => {
        // All changes happen here
        // If any fails, all rollback
    });
}
```

**Current Issue**: UnitOfWork exists but NOT registered in Program.cs!

---

### **STEP 9: Database - Entity Framework Core**

**What happens**: EF Core converts entities to SQL and executes.

```
Account entity → EF Core → SQL INSERT → SQL Server → Data saved
```

**Why EF Core?**
- **ORM**: Object-Relational Mapping (objects to tables)
- **Change Tracking**: Knows what changed
- **Migrations**: Version control for database schema

---

## 🔄 **COMPLETE FLOW DIAGRAM**

```
┌─────────────────────────────────────────────────────────────┐
│ 1. HTTP Request                                             │
│    POST /api/accounts/create                               │
│    Body: { customerId, accountType, initialDeposit }        │
└──────────────────────┬──────────────────────────────────────┘
                       ↓
┌─────────────────────────────────────────────────────────────┐
│ 2. AccountController                                        │
│    - Receives HTTP request                                  │
│    - Maps to CreateAccountCommand                           │
│    - Sends to MediatR: await _mediator.Send(command)       │
└──────────────────────┬──────────────────────────────────────┘
                       ↓
┌─────────────────────────────────────────────────────────────┐
│ 3. MediatR Pipeline                                         │
│    ┌────────────────────────────────────┐                   │
│    │ ValidationBehavior                │ ← Validates input │
│    └────────────┬───────────────────────┘                   │
│                 ↓                                              │
│    ┌────────────────────────────────────┐                   │
│    │ LoggingBehavior                    │ ← Logs request   │
│    └────────────┬───────────────────────┘                   │
│                 ↓                                              │
│    ┌────────────────────────────────────┐                   │
│    │ CreateAccountCommandHandler        │ ← Routes to this │
│    └────────────────────────────────────┘                   │
└──────────────────────┬──────────────────────────────────────┘
                       ↓
┌─────────────────────────────────────────────────────────────┐
│ 4. CreateAccountCommandHandler (Application Layer)          │
│    a. Validate customer exists                              │
│       → ICustomerRepository.GetByIdAsync()                  │
│    b. Generate unique account number                         │
│    c. Create account                                         │
│       → Account.Create()                                   │
│    d. Save account                                           │
│       → IAccountRepository.AddAsync()                       │
│    e. Commit transaction                                     │
│       → IUnitOfWork.SaveChangesAsync()                      │
└──────────────────────┬──────────────────────────────────────┘
                       ↓
┌─────────────────────────────────────────────────────────────┐
│ 5. Account.Create() (Domain Layer)                           │
│    - Validates initial balance (domain rules)                │
│    - Creates Account entity                                  │
│    - Raises AccountCreatedEvent                             │
│    - Returns Account entity                                  │
└──────────────────────┬──────────────────────────────────────┘
                       ↓
┌─────────────────────────────────────────────────────────────┐
│ 6. AccountRepository.AddAsync() (Infrastructure)            │
│    - Adds entity to EF Core change tracker                  │
│    - Does NOT save yet!                                     │
└──────────────────────┬──────────────────────────────────────┘
                       ↓
┌─────────────────────────────────────────────────────────────┐
│ 7. UnitOfWork.SaveChangesAsync() (Infrastructure)           │
│    - EF Core generates SQL                                  │
│    - Opens database transaction                             │
│    - Executes INSERT statement                               │
│    - Commits transaction                                    │
└──────────────────────┬──────────────────────────────────────┘
                       ↓
┌─────────────────────────────────────────────────────────────┐
│ 8. Result<Guid> returns back up the chain                    │
│    Handler → MediatR → Controller → HTTP Response           │
└─────────────────────────────────────────────────────────────┘
```

---

## 🎯 **PART 3: WHY EACH FILE EXISTS**

### **API Layer Files**

| File | Purpose | Why Needed | Node.js Equivalent |
|------|---------|-------------|-------------------|
| `Program.cs` | Application startup, service registration | Configures entire app, sets up DI | `app.js` / `server.js` |
| `AccountController.cs` | HTTP endpoints | Receives HTTP requests, returns HTTP responses | Express route handlers |

### **Application Layer Files**

| File | Purpose | Why Needed | Node.js Equivalent |
|------|---------|-------------|-------------------|
| `CreateAccountCommand.cs` | Command definition + handler | Encapsulates use case, separates commands from queries | Service methods |
| `TransferMoneyCommand.cs` | Money transfer use case | Another use case (command) | Service method |
| `GetTransactionHistoryQuery.cs` | Read transactions use case | Query (read operation) | Service method |
| `ValidationBehavior.cs` | Request validation middleware | Cross-cutting concern (all requests) | Express middleware |
| `LoggingBehavior.cs` | Request logging middleware | Cross-cutting concern (all requests) | Express middleware |
| `Result.cs` | Functional result pattern | Better error handling than exceptions | `{ success: boolean, data?, error? }` |

### **Domain Layer Files**

| File | Purpose | Why Needed | Node.js Equivalent |
|------|---------|-------------|-------------------|
| `Account.cs` | Account entity + business logic | Contains business rules, encapsulates state | Domain model class |
| `Customer.cs` | Customer entity | Domain entity | Domain model class |
| `Transaction.cs` | Transaction entity | Domain entity | Domain model class |
| `Money.cs` | Value object for money | Type safety, validation | Value object class |
| `AccountId.cs` | Strongly-typed ID | Type safety (prevents mixing IDs) | Typed ID class |
| `AccountCreatedEvent.cs` | Domain event | Event-driven architecture | Event emitter |

### **Infrastructure Layer Files**

| File | Purpose | Why Needed | Node.js Equivalent |
|------|---------|-------------|-------------------|
| `AccountRepository.cs` | Data access for accounts | Abstracts database, testable | Database client / Prisma |
| `UnitOfWork.cs` | Transaction management | Ensures atomicity | Database transaction |
| `BankingDbContext.cs` | EF Core context | Database connection, ORM | Prisma client / Mongoose |

---

## ⚠️ **CURRENT ISSUES & WHAT'S MISSING**

### **1. MediatR Not Registered**
**Problem**: Controllers can't use `IMediator`
**Fix**: Add to `Program.cs`:
```csharp
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(CreateAccountCommand).Assembly));
builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehaviour<,>));
```

### **2. UnitOfWork Not Registered**
**Problem**: Handlers can't inject `IUnitOfWork`
**Fix**: Add to `Program.cs`:
```csharp
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
```

### **3. Controller Not Using CQRS**
**Problem**: Controller directly uses repository
**Fix**: Refactor to use MediatR:
```csharp
private readonly IMediator _mediator;
await _mediator.Send(command);
```

### **4. Missing Project Reference**
**Problem**: API project can't see APP project (for MediatR)
**Fix**: Add to `CoreBanking.API.csproj`:
```xml
<ProjectReference Include="..\CoreBanking.APP\CoreBanking.APP.csproj" />
```

---

## 📖 **PART 4: KEY CONCEPTS EXPLAINED**

### **Dependency Injection (DI)**

**What**: Framework automatically provides dependencies

```csharp
// You declare what you need:
public CreateAccountCommandHandler(
    IAccountRepository accountRepository,  // ← Requested
    IUnitOfWork unitOfWork                 // ← Requested
)
{
    // Framework automatically provides these!
}

// Registered in Program.cs:
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
```

**Node.js Equivalent**: Dependency injection containers (InversifyJS, TypeDI)

---

### **Scoped Lifetime**

```csharp
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
```

**Meaning**: One instance per HTTP request
- Request 1 → Instance A
- Request 2 → Instance B
- Same request → Same instance

**Why**: Ensures consistency within a single request

---

### **Value Objects vs Entities**

**Entity** (`Account`):
- Has identity (AccountId)
- Can change over time
- Compared by ID

**Value Object** (`Money`):
- No identity
- Immutable (can't change)
- Compared by value

**Example**:
```csharp
var money1 = new Money(100, "USD");
var money2 = new Money(100, "USD");
money1 == money2  // ✅ True (same value)

var account1 = new Account(...);
var account2 = new Account(...);
account1 == account2  // ❌ False (different IDs)
```

---

### **Domain Events**

**What**: Something important happened in the domain

```csharp
account.AddDomainEvent(new AccountCreatedEvent(account));
```

**Why**:
- Decouple components
- Enable event-driven architecture
- Audit trail
- Integration with other systems

**Example Use Cases**:
- Send welcome email when account created
- Update reporting system
- Trigger notifications

---

## 🎓 **SUMMARY**

### **Request Flow (Ideal State)**
1. **HTTP Request** → Controller (thin layer)
2. **Controller** → MediatR (message bus)
3. **MediatR** → Behaviors (validation, logging)
4. **MediatR** → Handler (use case)
5. **Handler** → Domain (business logic)
6. **Handler** → Repository (data access)
7. **Handler** → UnitOfWork (save changes)
8. **Response** ← Goes back up the chain

### **Key Principles**
- **Separation of Concerns**: Each layer has one job
- **Dependency Inversion**: Depend on abstractions, not concretions
- **Single Responsibility**: One class, one reason to change
- **Encapsulation**: Hide implementation details

### **Why This Architecture?**
- **Testable**: Easy to test business logic
- **Maintainable**: Changes isolated to one layer
- **Scalable**: Can optimize each layer independently
- **Flexible**: Can swap implementations easily

---

## 🚀 **NEXT STEPS**

1. Register MediatR in `Program.cs`
2. Register UnitOfWork in `Program.cs`
3. Add project reference (API → APP)
4. Refactor Controller to use MediatR
5. Test the complete flow!


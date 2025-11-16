# C# Mastery Guide: From Beginner to Advanced Backend Developer

## Table of Contents
1. [Learning Roadmap Overview](#learning-roadmap-overview)
2. [Phase 1: C# Fundamentals (Weeks 1-4)](#phase-1-c-fundamentals-weeks-1-4)
3. [Phase 2: Object-Oriented Programming (Weeks 5-8)](#phase-2-object-oriented-programming-weeks-5-8)
4. [Phase 3: Advanced C# Features (Weeks 9-12)](#phase-3-advanced-c-features-weeks-9-12)
5. [Phase 4: Generics Deep Dive - Understanding Result&lt;T&gt;](#phase-4-generics-deep-dive---understanding-resultt)
6. [Phase 5: Asynchronous Programming (Weeks 13-16)](#phase-5-asynchronous-programming-weeks-13-16)
7. [Phase 6: LINQ Mastery (Weeks 17-18)](#phase-6-linq-mastery-weeks-17-18)
8. [Phase 7: Design Patterns & Architecture (Weeks 19-24)](#phase-7-design-patterns--architecture-weeks-19-24)
9. [Phase 8: Backend Development with ASP.NET Core (Weeks 25-32)](#phase-8-backend-development-with-aspnet-core-weeks-25-32)
10. [Mind Maps](#mind-maps)
11. [Practice Projects](#practice-projects)
12. [Feedback & Self-Assessment](#feedback--self-assessment)
13. [Resources & Learning Path](#resources--learning-path)

---

## Learning Roadmap Overview

```
YOUR JOURNEY TO C# MASTERY
═══════════════════════════

BEGINNER (Months 1-2)
├─ Variables & Types
├─ Control Flow
├─ Methods & Functions
└─ Basic OOP

INTERMEDIATE (Months 3-4)
├─ Advanced OOP
├─ Generics
├─ LINQ
└─ Async/Await

ADVANCED (Months 5-6)
├─ Design Patterns
├─ Architecture
├─ Performance
└─ Testing

EXPERT (Months 7-8)
├─ ASP.NET Core
├─ Database & EF Core
├─ Security
└─ Production Systems
```

---

## Phase 1: C# Fundamentals (Weeks 1-4)

### Week 1: Variables and Types

#### 1.1 Value Types vs Reference Types

This is FUNDAMENTAL. If you don't understand this, everything else will be confusing.

```csharp
// VALUE TYPES - Stored on STACK (fast, small, copied)
int age = 25;              // Integer (whole number)
double price = 19.99;      // Decimal with precision
bool isActive = true;      // Boolean (true/false)
char grade = 'A';          // Single character
decimal money = 1000.50m;  // Financial precision (use for money!)
float temperature = 98.6f; // Less precise decimal

// REFERENCE TYPES - Stored on HEAP (flexible, larger, referenced)
string name = "John";              // Text
int[] numbers = {1, 2, 3};         // Array
List<int> list = new List<int>();  // Collection
object obj = new object();         // Base of everything
```

**The Stack vs Heap Analogy:**
```
STACK (Value Types):              HEAP (Reference Types):
┌─────────────────┐               ┌─────────────────┐
│ Like sticky     │               │ Like a storage  │
│ notes on your   │               │ warehouse       │
│ desk            │               │                 │
│                 │               │ You have a KEY  │
│ Small, fast     │               │ (reference) to  │
│ access          │               │ find your stuff │
│                 │               │                 │
│ Gone when you   │               │ Stays until     │
│ leave desk      │               │ cleaned up      │
└─────────────────┘               └─────────────────┘
```

**Critical Example - Understanding Pass by Value vs Reference:**
```csharp
// VALUE TYPE - Copy is made
void ChangeValue(int x)
{
    x = 100;  // Only changes the COPY
}

int number = 5;
ChangeValue(number);
Console.WriteLine(number);  // Still 5! Original unchanged

// REFERENCE TYPE - Reference is passed
void ChangeList(List<int> list)
{
    list.Add(100);  // Modifies the ORIGINAL
}

var myList = new List<int> { 1, 2, 3 };
ChangeList(myList);
Console.WriteLine(myList.Count);  // 4! Original was modified
```

#### 1.2 Nullable Types

```csharp
// Non-nullable (cannot be null)
int count = 10;
// count = null;  // ERROR! Cannot be null

// Nullable value type (can be null)
int? maybeCount = null;  // OK!
maybeCount = 10;         // Also OK

// Working with nullables
if (maybeCount.HasValue)
{
    int actualValue = maybeCount.Value;
}

// Null coalescing operator
int safeCount = maybeCount ?? 0;  // Use 0 if null

// Null conditional operator
string? name = null;
int? length = name?.Length;  // null if name is null
```

#### 1.3 Type Conversion

```csharp
// Implicit conversion (safe, automatic)
int smallNumber = 100;
long bigNumber = smallNumber;  // int fits in long

// Explicit conversion (casting - may lose data)
double pi = 3.14159;
int rounded = (int)pi;  // 3 - decimal part lost!

// Safe conversion with TryParse
string input = "123";
if (int.TryParse(input, out int result))
{
    Console.WriteLine($"Parsed: {result}");
}
else
{
    Console.WriteLine("Invalid number");
}

// Convert class
string strNum = "456";
int converted = Convert.ToInt32(strNum);
```

### Week 2: Control Flow

#### 2.1 Conditionals

```csharp
// If-else
int score = 85;
string grade;

if (score >= 90)
    grade = "A";
else if (score >= 80)
    grade = "B";
else if (score >= 70)
    grade = "C";
else
    grade = "F";

// Ternary operator (short if-else)
string status = score >= 60 ? "Pass" : "Fail";

// Switch expression (C# 8+)
string dayType = DateTime.Now.DayOfWeek switch
{
    DayOfWeek.Saturday or DayOfWeek.Sunday => "Weekend",
    _ => "Weekday"
};

// Pattern matching switch
object value = 42;
string description = value switch
{
    int i when i > 0 => "Positive integer",
    int i when i < 0 => "Negative integer",
    int => "Zero",
    string s => $"String: {s}",
    null => "Null value",
    _ => "Unknown type"
};
```

#### 2.2 Loops

```csharp
// For loop - when you know count
for (int i = 0; i < 10; i++)
{
    Console.WriteLine(i);
}

// Foreach - iterate collections
var names = new List<string> { "Alice", "Bob", "Charlie" };
foreach (var name in names)
{
    Console.WriteLine(name);
}

// While - condition-based
int count = 0;
while (count < 5)
{
    Console.WriteLine(count);
    count++;
}

// Do-while - at least once
do
{
    Console.WriteLine("This runs at least once");
} while (false);

// Break and Continue
for (int i = 0; i < 10; i++)
{
    if (i == 3) continue;  // Skip 3
    if (i == 7) break;     // Stop at 7
    Console.WriteLine(i);  // Prints: 0, 1, 2, 4, 5, 6
}
```

### Week 3: Methods and Functions

#### 3.1 Method Basics

```csharp
public class Calculator
{
    // Basic method
    public int Add(int a, int b)
    {
        return a + b;
    }

    // Method with default parameters
    public decimal CalculateInterest(decimal principal, decimal rate = 0.05m, int years = 1)
    {
        return principal * rate * years;
    }

    // Method with out parameter
    public bool TryDivide(int a, int b, out int result)
    {
        if (b == 0)
        {
            result = 0;
            return false;
        }
        result = a / b;
        return true;
    }

    // Method with ref parameter
    public void Swap(ref int x, ref int y)
    {
        int temp = x;
        x = y;
        y = temp;
    }

    // Method with params (variable arguments)
    public int Sum(params int[] numbers)
    {
        int total = 0;
        foreach (var num in numbers)
            total += num;
        return total;
    }
}

// Usage
var calc = new Calculator();
int sum = calc.Add(5, 3);                              // 8
decimal interest = calc.CalculateInterest(1000);       // Uses defaults
decimal interest2 = calc.CalculateInterest(1000, 0.1m, 2);

if (calc.TryDivide(10, 3, out int result))
    Console.WriteLine(result);  // 3

int a = 5, b = 10;
calc.Swap(ref a, ref b);  // a=10, b=5

int total = calc.Sum(1, 2, 3, 4, 5);  // 15
```

#### 3.2 Expression-Bodied Members

```csharp
public class Person
{
    private string _firstName;
    private string _lastName;

    // Expression-bodied constructor
    public Person(string first, string last) => (_firstName, _lastName) = (first, last);

    // Expression-bodied property
    public string FullName => $"{_firstName} {_lastName}";

    // Expression-bodied method
    public string Greet() => $"Hello, {FullName}!";

    // Expression-bodied property with getter/setter
    public string FirstName
    {
        get => _firstName;
        set => _firstName = value ?? throw new ArgumentNullException(nameof(value));
    }
}
```

### Week 4: Collections

#### 4.1 Arrays

```csharp
// Fixed-size array
int[] numbers = new int[5];          // [0, 0, 0, 0, 0]
int[] primes = { 2, 3, 5, 7, 11 };   // Initialized
int[] odds = new int[] { 1, 3, 5 };  // Another syntax

// Access elements
int first = primes[0];  // 2
int last = primes[^1];  // 11 (from end)

// Slicing (C# 8+)
int[] middle = primes[1..4];  // [3, 5, 7]

// Multi-dimensional
int[,] matrix = new int[3, 3];
matrix[0, 0] = 1;

// Jagged array (array of arrays)
int[][] jagged = new int[3][];
jagged[0] = new int[] { 1, 2 };
jagged[1] = new int[] { 3, 4, 5 };
```

#### 4.2 Lists

```csharp
// Dynamic-size collection
List<string> names = new List<string>();
names.Add("Alice");
names.Add("Bob");
names.AddRange(new[] { "Charlie", "Diana" });

// Initialize with values
var numbers = new List<int> { 1, 2, 3, 4, 5 };

// Common operations
numbers.Remove(3);           // Remove value 3
numbers.RemoveAt(0);         // Remove at index 0
numbers.Insert(0, 10);       // Insert 10 at index 0
bool hasTwo = numbers.Contains(2);
int index = numbers.IndexOf(4);
numbers.Sort();
numbers.Reverse();
numbers.Clear();

// List with initial capacity (performance)
var bigList = new List<int>(1000);  // Pre-allocate space
```

#### 4.3 Dictionaries

```csharp
// Key-value pairs
var ages = new Dictionary<string, int>
{
    ["Alice"] = 25,
    ["Bob"] = 30,
    ["Charlie"] = 35
};

// Add/update
ages["Diana"] = 28;
ages.Add("Eve", 22);  // Throws if key exists

// Safe access
if (ages.TryGetValue("Frank", out int frankAge))
{
    Console.WriteLine(frankAge);
}
else
{
    Console.WriteLine("Frank not found");
}

// Check key exists
if (ages.ContainsKey("Alice"))
{
    Console.WriteLine(ages["Alice"]);
}

// Iterate
foreach (var kvp in ages)
{
    Console.WriteLine($"{kvp.Key}: {kvp.Value}");
}

// Get keys or values
var allNames = ages.Keys;
var allAges = ages.Values;
```

#### 4.4 Other Collections

```csharp
// HashSet - unique values, fast lookup
var uniqueNumbers = new HashSet<int> { 1, 2, 3 };
uniqueNumbers.Add(2);  // Ignored, already exists
bool hasThree = uniqueNumbers.Contains(3);  // Very fast O(1)

// Queue - First In, First Out (FIFO)
var queue = new Queue<string>();
queue.Enqueue("First");
queue.Enqueue("Second");
string first = queue.Dequeue();  // "First"

// Stack - Last In, First Out (LIFO)
var stack = new Stack<string>();
stack.Push("First");
stack.Push("Second");
string top = stack.Pop();  // "Second"

// SortedDictionary - automatically sorted by key
var sorted = new SortedDictionary<string, int>
{
    ["Zebra"] = 1,
    ["Apple"] = 2,
    ["Mango"] = 3
};
// Keys are: Apple, Mango, Zebra
```

---

## Phase 2: Object-Oriented Programming (Weeks 5-8)

### Week 5: Classes and Objects

#### 5.1 Class Fundamentals

```csharp
public class BankAccount
{
    // Fields (private by convention)
    private decimal _balance;
    private readonly string _accountNumber;  // Can only set in constructor
    private static int _totalAccounts = 0;   // Shared across all instances

    // Properties
    public string AccountNumber => _accountNumber;  // Read-only
    public decimal Balance => _balance;             // Read-only
    public string OwnerName { get; set; }           // Auto-property
    public DateTime CreatedAt { get; init; }        // Set only during initialization

    // Constructor
    public BankAccount(string accountNumber, string ownerName, decimal initialBalance = 0)
    {
        _accountNumber = accountNumber;
        OwnerName = ownerName;
        _balance = initialBalance;
        CreatedAt = DateTime.UtcNow;
        _totalAccounts++;
    }

    // Static constructor (runs once when class first used)
    static BankAccount()
    {
        Console.WriteLine("BankAccount class initialized");
    }

    // Methods
    public void Deposit(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be positive");

        _balance += amount;
    }

    public bool Withdraw(decimal amount)
    {
        if (amount <= 0 || amount > _balance)
            return false;

        _balance -= amount;
        return true;
    }

    // Static method
    public static int GetTotalAccounts() => _totalAccounts;

    // Override ToString
    public override string ToString()
    {
        return $"Account {_accountNumber}: {OwnerName}, Balance: {_balance:C}";
    }
}

// Usage
var account = new BankAccount("ACC001", "John Doe", 1000m)
{
    // Object initializer for init properties
};

account.Deposit(500);
bool success = account.Withdraw(200);
Console.WriteLine(account);  // Uses ToString()
Console.WriteLine(BankAccount.GetTotalAccounts());  // Static method
```

#### 5.2 Access Modifiers

```csharp
public class Example
{
    public int PublicField;           // Accessible everywhere
    private int _privateField;        // Only in this class
    protected int ProtectedField;     // This class and derived classes
    internal int InternalField;       // Same assembly (project)
    protected internal int ProtectedInternalField;  // Same assembly OR derived classes
    private protected int PrivateProtectedField;    // Same assembly AND derived classes
}
```

**Visual Guide:**
```
Access Level          Same Class    Derived Class    Same Assembly    Different Assembly
─────────────────────────────────────────────────────────────────────────────────────────
public                    ✓              ✓               ✓                  ✓
private                   ✓              ✗               ✗                  ✗
protected                 ✓              ✓               ✗                  ✗
internal                  ✓              ✓               ✓                  ✗
protected internal        ✓              ✓               ✓                  ✓ (if derived)
private protected         ✓              ✓ (same asm)    ✗                  ✗
```

### Week 6: Inheritance and Polymorphism

#### 6.1 Inheritance

```csharp
// Base class
public class Animal
{
    public string Name { get; set; }
    protected int Age { get; set; }  // Accessible in derived classes

    public Animal(string name, int age)
    {
        Name = name;
        Age = age;
    }

    public virtual void Speak()  // Virtual = can be overridden
    {
        Console.WriteLine($"{Name} makes a sound");
    }

    public void Sleep()  // Not virtual = cannot be overridden
    {
        Console.WriteLine($"{Name} is sleeping");
    }
}

// Derived class
public class Dog : Animal
{
    public string Breed { get; set; }

    public Dog(string name, int age, string breed)
        : base(name, age)  // Call base constructor
    {
        Breed = breed;
    }

    public override void Speak()  // Override base method
    {
        Console.WriteLine($"{Name} barks: Woof!");
    }

    public void Fetch()  // New method specific to Dog
    {
        Console.WriteLine($"{Name} fetches the ball");
    }
}

public class Cat : Animal
{
    public Cat(string name, int age) : base(name, age) { }

    public override void Speak()
    {
        Console.WriteLine($"{Name} meows: Meow!");
    }
}

// Usage - Polymorphism
List<Animal> animals = new List<Animal>
{
    new Dog("Rex", 3, "Labrador"),
    new Cat("Whiskers", 5),
    new Dog("Buddy", 2, "Beagle")
};

foreach (var animal in animals)
{
    animal.Speak();  // Calls appropriate override
    // Rex barks: Woof!
    // Whiskers meows: Meow!
    // Buddy barks: Woof!
}
```

#### 6.2 Abstract Classes

```csharp
// Cannot be instantiated, only inherited
public abstract class Shape
{
    public string Color { get; set; }

    // Abstract method - MUST be implemented
    public abstract double CalculateArea();

    // Virtual method - CAN be overridden
    public virtual void Draw()
    {
        Console.WriteLine($"Drawing a {Color} shape");
    }

    // Regular method
    public void Describe()
    {
        Console.WriteLine($"This is a {Color} shape with area {CalculateArea()}");
    }
}

public class Circle : Shape
{
    public double Radius { get; set; }

    public override double CalculateArea()
    {
        return Math.PI * Radius * Radius;
    }

    public override void Draw()
    {
        Console.WriteLine($"Drawing a {Color} circle with radius {Radius}");
    }
}

public class Rectangle : Shape
{
    public double Width { get; set; }
    public double Height { get; set; }

    public override double CalculateArea()
    {
        return Width * Height;
    }
}

// Usage
// var shape = new Shape();  // ERROR! Cannot instantiate abstract class
var circle = new Circle { Color = "Red", Radius = 5 };
circle.Describe();  // Uses inherited method + implemented abstract method
```

### Week 7: Interfaces

#### 7.1 Interface Basics

```csharp
// Interface = Contract (what you can do)
public interface IPaymentProcessor
{
    // Methods (no implementation in traditional interface)
    bool ProcessPayment(decimal amount);
    void RefundPayment(string transactionId);

    // Properties
    string ProcessorName { get; }
    bool IsAvailable { get; }

    // Default implementation (C# 8+)
    void LogTransaction(string message)
    {
        Console.WriteLine($"[{ProcessorName}] {message}");
    }
}

// Implementing interface
public class StripeProcessor : IPaymentProcessor
{
    public string ProcessorName => "Stripe";
    public bool IsAvailable => true;

    public bool ProcessPayment(decimal amount)
    {
        Console.WriteLine($"Processing ${amount} via Stripe");
        // Stripe API call here
        return true;
    }

    public void RefundPayment(string transactionId)
    {
        Console.WriteLine($"Refunding transaction {transactionId}");
    }
}

public class PayPalProcessor : IPaymentProcessor
{
    public string ProcessorName => "PayPal";
    public bool IsAvailable => true;

    public bool ProcessPayment(decimal amount)
    {
        Console.WriteLine($"Processing ${amount} via PayPal");
        return true;
    }

    public void RefundPayment(string transactionId)
    {
        Console.WriteLine($"PayPal refund: {transactionId}");
    }
}

// Usage - Program to interface, not implementation
public class PaymentService
{
    private readonly IPaymentProcessor _processor;

    public PaymentService(IPaymentProcessor processor)
    {
        _processor = processor;  // Can be Stripe, PayPal, or any implementation
    }

    public void MakePayment(decimal amount)
    {
        if (_processor.IsAvailable)
        {
            _processor.ProcessPayment(amount);
            _processor.LogTransaction($"Processed {amount}");
        }
    }
}

// Can swap implementations easily
var stripeService = new PaymentService(new StripeProcessor());
var paypalService = new PaymentService(new PayPalProcessor());
```

#### 7.2 Multiple Interface Implementation

```csharp
public interface IReadable
{
    string Read();
}

public interface IWritable
{
    void Write(string content);
}

public interface IDeletable
{
    void Delete();
}

// Class can implement multiple interfaces
public class File : IReadable, IWritable, IDeletable
{
    private string _content = "";

    public string Read() => _content;

    public void Write(string content) => _content = content;

    public void Delete() => _content = "";
}

// Interface inheritance
public interface IFileOperations : IReadable, IWritable, IDeletable
{
    void Rename(string newName);
}
```

### Week 8: SOLID Principles

#### 8.1 Single Responsibility Principle (SRP)

```csharp
// BAD - Multiple responsibilities
public class User
{
    public string Name { get; set; }
    public string Email { get; set; }

    public void SaveToDatabase() { /* DB logic */ }
    public void SendEmail(string message) { /* Email logic */ }
    public string GenerateReport() { /* Report logic */ }
}

// GOOD - Each class has one responsibility
public class User
{
    public string Name { get; set; }
    public string Email { get; set; }
}

public class UserRepository
{
    public void Save(User user) { /* DB logic */ }
}

public class EmailService
{
    public void Send(string to, string message) { /* Email logic */ }
}

public class UserReportGenerator
{
    public string Generate(User user) { /* Report logic */ }
}
```

#### 8.2 Open/Closed Principle (OCP)

```csharp
// BAD - Must modify class to add new shape
public class AreaCalculator
{
    public double Calculate(object shape)
    {
        if (shape is Circle c)
            return Math.PI * c.Radius * c.Radius;
        else if (shape is Rectangle r)
            return r.Width * r.Height;
        // Must add new if for each new shape!
        return 0;
    }
}

// GOOD - Open for extension, closed for modification
public interface IShape
{
    double CalculateArea();
}

public class Circle : IShape
{
    public double Radius { get; set; }
    public double CalculateArea() => Math.PI * Radius * Radius;
}

public class Rectangle : IShape
{
    public double Width { get; set; }
    public double Height { get; set; }
    public double CalculateArea() => Width * Height;
}

public class Triangle : IShape  // New shape, no modification needed
{
    public double Base { get; set; }
    public double Height { get; set; }
    public double CalculateArea() => 0.5 * Base * Height;
}

public class AreaCalculator
{
    public double Calculate(IShape shape) => shape.CalculateArea();
}
```

#### 8.3 Liskov Substitution Principle (LSP)

```csharp
// BAD - Square violates LSP
public class Rectangle
{
    public virtual int Width { get; set; }
    public virtual int Height { get; set; }

    public int Area() => Width * Height;
}

public class Square : Rectangle
{
    public override int Width
    {
        set { base.Width = base.Height = value; }  // Unexpected behavior!
    }

    public override int Height
    {
        set { base.Width = base.Height = value; }  // Unexpected behavior!
    }
}

// This breaks:
Rectangle rect = new Square();
rect.Width = 5;
rect.Height = 10;
Console.WriteLine(rect.Area());  // Expected 50, got 100!

// GOOD - Separate hierarchy
public interface IShape
{
    int Area();
}

public class Rectangle : IShape
{
    public int Width { get; set; }
    public int Height { get; set; }
    public int Area() => Width * Height;
}

public class Square : IShape
{
    public int Side { get; set; }
    public int Area() => Side * Side;
}
```

#### 8.4 Interface Segregation Principle (ISP)

```csharp
// BAD - Fat interface
public interface IWorker
{
    void Work();
    void Eat();
    void Sleep();
}

public class Human : IWorker
{
    public void Work() { /* works */ }
    public void Eat() { /* eats */ }
    public void Sleep() { /* sleeps */ }
}

public class Robot : IWorker
{
    public void Work() { /* works */ }
    public void Eat() { throw new NotImplementedException(); }  // Robots don't eat!
    public void Sleep() { throw new NotImplementedException(); }  // Robots don't sleep!
}

// GOOD - Segregated interfaces
public interface IWorkable
{
    void Work();
}

public interface IFeedable
{
    void Eat();
}

public interface ISleepable
{
    void Sleep();
}

public class Human : IWorkable, IFeedable, ISleepable
{
    public void Work() { }
    public void Eat() { }
    public void Sleep() { }
}

public class Robot : IWorkable
{
    public void Work() { }
    // No need to implement Eat or Sleep!
}
```

#### 8.5 Dependency Inversion Principle (DIP)

```csharp
// BAD - High-level depends on low-level
public class EmailSender
{
    public void Send(string message) { /* sends email */ }
}

public class NotificationService
{
    private EmailSender _emailSender = new EmailSender();  // Direct dependency!

    public void Notify(string message)
    {
        _emailSender.Send(message);
    }
}

// GOOD - Both depend on abstraction
public interface IMessageSender
{
    void Send(string message);
}

public class EmailSender : IMessageSender
{
    public void Send(string message) { /* sends email */ }
}

public class SmsSender : IMessageSender
{
    public void Send(string message) { /* sends SMS */ }
}

public class NotificationService
{
    private readonly IMessageSender _sender;

    public NotificationService(IMessageSender sender)  // Inject dependency
    {
        _sender = sender;
    }

    public void Notify(string message)
    {
        _sender.Send(message);
    }
}

// Usage - Easily swap implementations
var emailNotifier = new NotificationService(new EmailSender());
var smsNotifier = new NotificationService(new SmsSender());
```

---

## Phase 3: Advanced C# Features (Weeks 9-12)

### Week 9: Records and Init-Only Properties

#### 9.1 Records (C# 9+)

```csharp
// Record - Immutable reference type with value semantics
public record Person(string FirstName, string LastName, int Age);

// Usage
var person1 = new Person("John", "Doe", 30);
var person2 = new Person("John", "Doe", 30);

// Value equality (not reference equality)
Console.WriteLine(person1 == person2);  // True!
Console.WriteLine(person1.Equals(person2));  // True!

// Immutable - cannot change
// person1.Age = 31;  // ERROR!

// Create modified copy with "with"
var olderPerson = person1 with { Age = 31 };
Console.WriteLine(person1.Age);  // 30 (unchanged)
Console.WriteLine(olderPerson.Age);  // 31 (new instance)

// Deconstruction
var (first, last, age) = person1;
Console.WriteLine($"{first} {last} is {age}");

// Built-in ToString
Console.WriteLine(person1);  // Person { FirstName = John, LastName = Doe, Age = 30 }

// Record with additional members
public record Employee(string Name, string Department, decimal Salary)
{
    // Additional property
    public string EmployeeId { get; init; } = Guid.NewGuid().ToString();

    // Additional method
    public decimal CalculateBonus() => Salary * 0.1m;
}

// Record class vs Record struct
public record class ClassRecord(string Name);  // Reference type (default)
public record struct StructRecord(string Name);  // Value type
public readonly record struct ImmutableStructRecord(string Name);  // Readonly value type
```

#### 9.2 Init-Only Properties

```csharp
public class Configuration
{
    public string ConnectionString { get; init; }
    public int MaxRetries { get; init; }
    public TimeSpan Timeout { get; init; }

    // Can only be set during object initialization
}

// Usage
var config = new Configuration
{
    ConnectionString = "Server=localhost",
    MaxRetries = 3,
    Timeout = TimeSpan.FromSeconds(30)
};

// Cannot change after initialization
// config.MaxRetries = 5;  // ERROR!

// But can create new object with different values
var newConfig = new Configuration
{
    ConnectionString = config.ConnectionString,
    MaxRetries = 5,
    Timeout = config.Timeout
};
```

### Week 10: Pattern Matching

#### 10.1 Type Patterns

```csharp
object data = GetSomeData();

// Type pattern with declaration
if (data is string text)
{
    Console.WriteLine($"Text: {text.ToUpper()}");
}

// Type pattern in switch
string result = data switch
{
    string s => $"String: {s}",
    int i => $"Integer: {i}",
    double d => $"Double: {d:F2}",
    bool b => $"Boolean: {b}",
    null => "Null",
    _ => "Unknown type"
};
```

#### 10.2 Property Patterns

```csharp
public record Address(string City, string Country, string PostalCode);
public record Customer(string Name, Address Address, decimal Balance);

var customer = new Customer("John", new Address("New York", "USA", "10001"), 5000);

// Property pattern
if (customer is { Balance: > 1000, Address.Country: "USA" })
{
    Console.WriteLine("High-value US customer");
}

// In switch expression
string category = customer switch
{
    { Balance: >= 10000 } => "Premium",
    { Balance: >= 5000 } => "Gold",
    { Balance: >= 1000 } => "Silver",
    { Balance: > 0 } => "Basic",
    _ => "Inactive"
};

// Nested property patterns
string shipping = customer switch
{
    { Address: { Country: "USA", City: "New York" } } => "Same-day delivery",
    { Address: { Country: "USA" } } => "2-day shipping",
    { Address: { Country: "Canada" or "Mexico" } } => "5-day shipping",
    _ => "International shipping"
};
```

#### 10.3 Relational and Logical Patterns

```csharp
int temperature = 75;

string weather = temperature switch
{
    < 32 => "Freezing",
    >= 32 and < 50 => "Cold",
    >= 50 and < 70 => "Cool",
    >= 70 and < 85 => "Warm",
    >= 85 => "Hot"
};

// Logical patterns with negation
if (data is not null)
{
    // Do something
}

if (data is not string and not int)
{
    // Neither string nor int
}

// Parenthesized patterns for complex logic
string classification = temperature switch
{
    (>= 0 and < 32) or (> 100) => "Extreme",
    _ => "Normal"
};
```

#### 10.4 List Patterns (C# 11+)

```csharp
int[] numbers = { 1, 2, 3, 4, 5 };

// Match exact sequence
if (numbers is [1, 2, 3, 4, 5])
{
    Console.WriteLine("Exact match!");
}

// Match with wildcards
if (numbers is [1, _, _, _, 5])
{
    Console.WriteLine("Starts with 1, ends with 5");
}

// Capture elements
if (numbers is [var first, .., var last])
{
    Console.WriteLine($"First: {first}, Last: {last}");
}

// Match patterns in list
string description = numbers switch
{
    [] => "Empty",
    [var single] => $"Single element: {single}",
    [var a, var b] => $"Two elements: {a}, {b}",
    [1, .., 5] => "Starts with 1, ends with 5",
    { Length: > 10 } => "Large array",
    _ => "Other"
};
```

### Week 11: Delegates and Events

#### 11.1 Delegates

```csharp
// Delegate = Type-safe function pointer
public delegate int MathOperation(int a, int b);

public class Calculator
{
    public int Add(int a, int b) => a + b;
    public int Subtract(int a, int b) => a - b;
    public int Multiply(int a, int b) => a * b;
    public int Divide(int a, int b) => a / b;
}

// Usage
var calc = new Calculator();
MathOperation operation = calc.Add;
int result = operation(5, 3);  // 8

operation = calc.Multiply;
result = operation(5, 3);  // 15

// Multicast delegate
MathOperation operations = calc.Add;
operations += calc.Multiply;  // Chain multiple methods

// Built-in delegates
Func<int, int, int> add = (a, b) => a + b;  // Returns value
Action<string> print = msg => Console.WriteLine(msg);  // No return value
Predicate<int> isEven = n => n % 2 == 0;  // Returns bool

// Using Func as parameter
public List<T> Filter<T>(List<T> items, Func<T, bool> predicate)
{
    var result = new List<T>();
    foreach (var item in items)
    {
        if (predicate(item))
            result.Add(item);
    }
    return result;
}

var numbers = new List<int> { 1, 2, 3, 4, 5, 6 };
var evens = Filter(numbers, n => n % 2 == 0);  // [2, 4, 6]
```

#### 11.2 Lambda Expressions

```csharp
// Lambda = Anonymous function

// Expression lambda
Func<int, int> square = x => x * x;

// Statement lambda
Func<int, int> factorial = n =>
{
    int result = 1;
    for (int i = 2; i <= n; i++)
        result *= i;
    return result;
};

// Multiple parameters
Func<int, int, int> add = (a, b) => a + b;

// No parameters
Func<DateTime> now = () => DateTime.Now;

// Discard unused parameters
Action<int, int> printFirst = (first, _) => Console.WriteLine(first);

// Lambda with LINQ
var numbers = new List<int> { 1, 2, 3, 4, 5 };
var doubled = numbers.Select(n => n * 2).ToList();
var evens = numbers.Where(n => n % 2 == 0).ToList();
var sum = numbers.Aggregate((a, b) => a + b);

// Closure - Lambda captures outer variable
int multiplier = 3;
Func<int, int> multiply = n => n * multiplier;
Console.WriteLine(multiply(5));  // 15
```

#### 11.3 Events

```csharp
// Event = Publish-subscribe pattern

// Event arguments
public class BalanceChangedEventArgs : EventArgs
{
    public decimal OldBalance { get; }
    public decimal NewBalance { get; }
    public decimal Change => NewBalance - OldBalance;

    public BalanceChangedEventArgs(decimal oldBalance, decimal newBalance)
    {
        OldBalance = oldBalance;
        NewBalance = newBalance;
    }
}

// Publisher
public class BankAccount
{
    private decimal _balance;

    // Event declaration
    public event EventHandler<BalanceChangedEventArgs>? BalanceChanged;

    public decimal Balance
    {
        get => _balance;
        private set
        {
            var oldBalance = _balance;
            _balance = value;
            OnBalanceChanged(oldBalance, value);
        }
    }

    // Raise event
    protected virtual void OnBalanceChanged(decimal oldBalance, decimal newBalance)
    {
        BalanceChanged?.Invoke(this, new BalanceChangedEventArgs(oldBalance, newBalance));
    }

    public void Deposit(decimal amount) => Balance += amount;
    public void Withdraw(decimal amount) => Balance -= amount;
}

// Subscribers
public class NotificationService
{
    public void OnBalanceChanged(object? sender, BalanceChangedEventArgs e)
    {
        Console.WriteLine($"Balance changed from {e.OldBalance:C} to {e.NewBalance:C}");
    }
}

public class AuditLogger
{
    public void LogChange(object? sender, BalanceChangedEventArgs e)
    {
        Console.WriteLine($"AUDIT: Balance change of {e.Change:C}");
    }
}

// Usage
var account = new BankAccount();
var notifier = new NotificationService();
var logger = new AuditLogger();

// Subscribe to event
account.BalanceChanged += notifier.OnBalanceChanged;
account.BalanceChanged += logger.LogChange;

// Trigger event
account.Deposit(1000);
// Output:
// Balance changed from $0.00 to $1,000.00
// AUDIT: Balance change of $1,000.00

// Unsubscribe
account.BalanceChanged -= notifier.OnBalanceChanged;
```

### Week 12: Exception Handling

#### 12.1 Exception Basics

```csharp
public class SafeDivision
{
    public decimal Divide(decimal a, decimal b)
    {
        try
        {
            if (b == 0)
                throw new DivideByZeroException("Cannot divide by zero");

            return a / b;
        }
        catch (DivideByZeroException ex)
        {
            Console.WriteLine($"Division error: {ex.Message}");
            throw;  // Re-throw to caller
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error: {ex.Message}");
            throw;
        }
        finally
        {
            // Always runs, even if exception thrown
            Console.WriteLine("Division operation completed");
        }
    }
}

// Exception filters
try
{
    // Some code
}
catch (Exception ex) when (ex.Message.Contains("timeout"))
{
    // Only catches exceptions with "timeout" in message
}
catch (Exception ex) when (DateTime.Now.Hour >= 9 && DateTime.Now.Hour <= 17)
{
    // Only during business hours!
}
```

#### 12.2 Custom Exceptions

```csharp
// Custom exception
public class InsufficientFundsException : Exception
{
    public decimal Balance { get; }
    public decimal AttemptedWithdrawal { get; }

    public InsufficientFundsException(decimal balance, decimal attempted)
        : base($"Insufficient funds. Balance: {balance:C}, Attempted: {attempted:C}")
    {
        Balance = balance;
        AttemptedWithdrawal = attempted;
    }

    public InsufficientFundsException(string message) : base(message) { }

    public InsufficientFundsException(string message, Exception inner)
        : base(message, inner) { }
}

// Usage
public class Account
{
    public decimal Balance { get; private set; }

    public void Withdraw(decimal amount)
    {
        if (amount > Balance)
            throw new InsufficientFundsException(Balance, amount);

        Balance -= amount;
    }
}

try
{
    var account = new Account { Balance = 100 };
    account.Withdraw(150);
}
catch (InsufficientFundsException ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    Console.WriteLine($"You need {ex.AttemptedWithdrawal - ex.Balance:C} more");
}
```

---

## Phase 4: Generics Deep Dive - Understanding Result&lt;T&gt;

This is CRITICAL for understanding your codebase. Let's break it down completely.

### What are Generics?

Generics let you write code that works with ANY type. The `T` is a placeholder.

```csharp
// WITHOUT generics - must write multiple versions
public class IntegerBox
{
    private int _value;
    public void Store(int value) => _value = value;
    public int Retrieve() => _value;
}

public class StringBox
{
    private string _value;
    public void Store(string value) => _value = value;
    public string Retrieve() => _value;
}

// Repetitive! What if we need DecimalBox, DateTimeBox, etc.?

// WITH generics - ONE class works for ALL types
public class Box<T>  // T is a type parameter
{
    private T _value;
    public void Store(T value) => _value = value;
    public T Retrieve() => _value;
}

// Usage
var intBox = new Box<int>();
intBox.Store(42);
int number = intBox.Retrieve();

var stringBox = new Box<string>();
stringBox.Store("Hello");
string text = stringBox.Retrieve();

var dateBox = new Box<DateTime>();
dateBox.Store(DateTime.Now);
DateTime date = dateBox.Retrieve();
```

### Understanding Result&lt;T&gt; Step by Step

#### Step 1: The Problem (Without Result Pattern)

```csharp
// BAD - Using exceptions for control flow
public Account GetAccount(string id)
{
    var account = _repository.Find(id);
    if (account == null)
        throw new AccountNotFoundException(id);  // Exceptions are expensive!
    return account;
}

// BAD - Returning null
public Account? GetAccount(string id)
{
    return _repository.Find(id);  // What if null? Caller must check!
}

// Usage is error-prone:
var account = GetAccount("123");
account.Deposit(100);  // BOOM! NullReferenceException if not found

// BAD - Returning tuple
public (bool success, Account? account, string? error) GetAccount(string id)
{
    var account = _repository.Find(id);
    if (account == null)
        return (false, null, "Not found");
    return (true, account, null);
}

// Ugly usage:
var (success, account, error) = GetAccount("123");
if (success)
{
    account!.Deposit(100);  // Still need null check
}
```

#### Step 2: The Result Pattern

```csharp
// Result = Success OR Failure (never both)
public class Result<T>
{
    // Private constructor - can only create via factory methods
    private Result(T value)
    {
        IsSuccess = true;
        Value = value;
        Error = string.Empty;
    }

    private Result(string error)
    {
        IsSuccess = false;
        Value = default!;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T Value { get; }
    public string Error { get; }

    // Factory methods
    public static Result<T> Success(T value) => new Result<T>(value);
    public static Result<T> Failure(string error) => new Result<T>(error);

    // Implicit conversion (optional convenience)
    public static implicit operator Result<T>(T value) => Success(value);
}

// Non-generic version for operations without return value
public class Result
{
    private Result(bool success, string error)
    {
        IsSuccess = success;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string Error { get; }

    public static Result Success() => new Result(true, string.Empty);
    public static Result Failure(string error) => new Result(false, error);
}
```

#### Step 3: Using Result&lt;T&gt;

```csharp
public class AccountService
{
    private readonly IAccountRepository _repository;

    // Returns Result<Account> - either success with account, or failure with error
    public Result<Account> GetAccount(string id)
    {
        var account = _repository.Find(id);

        if (account == null)
            return Result<Account>.Failure("Account not found");

        if (!account.IsActive)
            return Result<Account>.Failure("Account is inactive");

        return Result<Account>.Success(account);
    }

    // Returns Result<Guid> - either success with new ID, or failure with error
    public Result<Guid> CreateAccount(string customerId, decimal initialBalance)
    {
        if (initialBalance < 0)
            return Result<Guid>.Failure("Initial balance cannot be negative");

        if (initialBalance > 1_000_000)
            return Result<Guid>.Failure("Initial balance exceeds maximum");

        var account = new Account(customerId, initialBalance);
        _repository.Add(account);

        return Result<Guid>.Success(account.Id);
    }

    // Returns Result (no value) - either success or failure
    public Result Transfer(string fromId, string toId, decimal amount)
    {
        var fromResult = GetAccount(fromId);
        if (fromResult.IsFailure)
            return Result.Failure($"Source account error: {fromResult.Error}");

        var toResult = GetAccount(toId);
        if (toResult.IsFailure)
            return Result.Failure($"Destination account error: {toResult.Error}");

        var from = fromResult.Value;
        var to = toResult.Value;

        if (from.Balance < amount)
            return Result.Failure("Insufficient funds");

        from.Withdraw(amount);
        to.Deposit(amount);

        return Result.Success();
    }
}

// Usage in controller
public class AccountController
{
    private readonly AccountService _service;

    public IActionResult GetAccount(string id)
    {
        var result = _service.GetAccount(id);

        if (result.IsFailure)
            return NotFound(result.Error);  // 404 with error message

        return Ok(result.Value);  // 200 with account data
    }

    public IActionResult CreateAccount(CreateAccountRequest request)
    {
        var result = _service.CreateAccount(request.CustomerId, request.InitialBalance);

        if (result.IsFailure)
            return BadRequest(result.Error);  // 400 with error message

        return Created($"/accounts/{result.Value}", result.Value);  // 201 with ID
    }

    public IActionResult Transfer(TransferRequest request)
    {
        var result = _service.Transfer(request.From, request.To, request.Amount);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok("Transfer completed");
    }
}
```

#### Step 4: Advanced Result Pattern

```csharp
// Result with multiple errors
public class Result<T>
{
    private Result(T value)
    {
        IsSuccess = true;
        Value = value;
        Errors = Array.Empty<string>();
    }

    private Result(IEnumerable<string> errors)
    {
        IsSuccess = false;
        Value = default!;
        Errors = errors.ToArray();
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T Value { get; }
    public string[] Errors { get; }
    public string Error => string.Join(", ", Errors);

    public static Result<T> Success(T value) => new Result<T>(value);
    public static Result<T> Failure(string error) => new Result<T>(new[] { error });
    public static Result<T> Failure(IEnumerable<string> errors) => new Result<T>(errors);

    // Map - Transform success value
    public Result<TNew> Map<TNew>(Func<T, TNew> mapper)
    {
        if (IsFailure)
            return Result<TNew>.Failure(Errors);

        return Result<TNew>.Success(mapper(Value));
    }

    // Bind - Chain operations
    public Result<TNew> Bind<TNew>(Func<T, Result<TNew>> binder)
    {
        if (IsFailure)
            return Result<TNew>.Failure(Errors);

        return binder(Value);
    }

    // Match - Handle both cases
    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<string[], TResult> onFailure)
    {
        return IsSuccess ? onSuccess(Value) : onFailure(Errors);
    }

    // OnSuccess - Execute if successful
    public Result<T> OnSuccess(Action<T> action)
    {
        if (IsSuccess)
            action(Value);
        return this;
    }

    // OnFailure - Execute if failed
    public Result<T> OnFailure(Action<string[]> action)
    {
        if (IsFailure)
            action(Errors);
        return this;
    }
}

// Usage of advanced features
var result = accountService.GetAccount("123")
    .OnSuccess(account => Console.WriteLine($"Found: {account.Id}"))
    .OnFailure(errors => Console.WriteLine($"Errors: {string.Join(", ", errors)}"))
    .Map(account => account.Balance)  // Result<Account> → Result<decimal>
    .Bind(balance => balance > 0
        ? Result<string>.Success("Positive balance")
        : Result<string>.Failure("Zero or negative balance"));

// Pattern matching with Result
var message = result.Match(
    onSuccess: value => $"Success: {value}",
    onFailure: errors => $"Failed: {string.Join(", ", errors)}"
);

// Chaining operations
public Result<AccountSummary> GetAccountSummary(string customerId, string accountId)
{
    return _customerService.GetCustomer(customerId)
        .Bind(customer => _accountService.GetAccount(accountId)
            .Map(account => new AccountSummary
            {
                CustomerName = customer.Name,
                AccountNumber = account.Number,
                Balance = account.Balance
            }));
}
```

#### Step 5: Result in Your Codebase

```csharp
// In CoreBanking.APP/Common/Models/Result.cs
public class Result<T>
{
    public bool IsSuccess { get; }
    public T Value { get; }
    public string Error { get; }

    // Your handlers return this:
    public class CreateAccountCommandHandler : IRequestHandler<CreateAccountCommand, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(CreateAccountCommand request, CancellationToken token)
        {
            // Validation
            if (request.InitialDeposit < 0)
                return Result<Guid>.Failure("Invalid deposit amount");

            // Business logic
            var customer = await _repository.GetByIdAsync(request.CustomerId);
            if (customer == null)
                return Result<Guid>.Failure("Customer not found");

            // Success path
            var account = Account.Create(...);
            await _repository.AddAsync(account);
            await _unitOfWork.SaveChangesAsync();

            return Result<Guid>.Success(account.Id.Value);
            // The <Guid> means: if successful, Value will be a Guid
        }
    }
}

// In controller
public async Task<ActionResult<ApiResponse<Guid>>> CreateAccount(CreateAccountRequest request)
{
    var command = _mapper.Map<CreateAccountCommand>(request);
    Result<Guid> result = await _mediator.Send(command);
    // result.Value is Guid (the account ID)

    if (result.IsFailure)
        return BadRequest(ApiResponse<Guid>.CreateFailure(result.Error));

    return Ok(ApiResponse<Guid>.CreateSuccess(result.Value));
}
```

### Generic Constraints

```csharp
// Unconstrained - T can be ANY type
public class Box<T>
{
    public T Value { get; set; }
}

// Constrained - T must meet certain criteria

// Must be a class (reference type)
public class Repository<T> where T : class
{
    public T? Find(int id) { /* ... */ }
}

// Must be a struct (value type)
public class ValueWrapper<T> where T : struct
{
    public T Value { get; set; }
}

// Must have parameterless constructor
public class Factory<T> where T : new()
{
    public T Create() => new T();
}

// Must implement interface
public class Sorter<T> where T : IComparable<T>
{
    public void Sort(List<T> items) => items.Sort();
}

// Must inherit from base class
public class AnimalShelter<T> where T : Animal
{
    public void Shelter(T animal) { /* ... */ }
}

// Multiple constraints
public class Repository<T> where T : class, IEntity, new()
{
    public T Create()
    {
        var entity = new T();
        entity.Id = Guid.NewGuid();
        return entity;
    }
}

// Multiple type parameters with different constraints
public class Converter<TSource, TTarget>
    where TSource : class
    where TTarget : class, new()
{
    public TTarget Convert(TSource source)
    {
        var target = new TTarget();
        // Map properties
        return target;
    }
}

// Your Result<T> could have constraints:
public class Result<T> where T : notnull  // T cannot be null
{
    // ...
}
```

---

## Phase 5: Asynchronous Programming (Weeks 13-16)

### Week 13: Understanding Async/Await

#### The Problem with Synchronous Code

```csharp
// SYNCHRONOUS (blocking)
public string DownloadWebPage(string url)
{
    var client = new WebClient();
    return client.DownloadString(url);  // Thread BLOCKS until complete
}

// While downloading, thread does NOTHING
// If this is the UI thread, app freezes
// If this is a web server thread, can't handle other requests
```

#### The Solution: Async/Await

```csharp
// ASYNCHRONOUS (non-blocking)
public async Task<string> DownloadWebPageAsync(string url)
{
    var client = new HttpClient();
    return await client.GetStringAsync(url);  // Thread released during I/O
}

// While downloading:
// - Thread is FREE to do other work
// - UI stays responsive
// - Server can handle other requests
```

#### How Async/Await Works (The Restaurant Analogy)

```
SYNCHRONOUS RESTAURANT:
Waiter takes order → Goes to kitchen → WAITS there → Brings food
(Waiter does nothing while cooking)

ASYNCHRONOUS RESTAURANT:
Waiter takes order → Goes to kitchen → Comes back to take more orders
Kitchen rings bell when ready → Waiter brings food
(Waiter serves multiple tables efficiently)
```

#### Basic Async Patterns

```csharp
// Method must be marked async
// Return type is Task<T> or Task (not T or void)
// Use await for async operations

public class DataService
{
    // Returns Task<T> - async method with return value
    public async Task<User> GetUserAsync(int id)
    {
        var user = await _database.FindAsync(id);  // await async operation
        return user;  // Returns User, not Task<User>
    }

    // Returns Task - async method without return value
    public async Task SaveUserAsync(User user)
    {
        await _database.SaveAsync(user);
        // No return statement needed
    }

    // Returns Task (void-returning async)
    public async Task ProcessAsync()
    {
        await Task.Delay(1000);  // Wait 1 second asynchronously
        Console.WriteLine("Done");
    }

    // ValueTask<T> - more efficient for methods that often complete synchronously
    public async ValueTask<User> GetCachedUserAsync(int id)
    {
        if (_cache.TryGet(id, out User user))
            return user;  // Returns immediately, no allocation

        return await _database.FindAsync(id);  // Async path
    }
}
```

### Week 14: Advanced Async Patterns

#### Running Tasks in Parallel

```csharp
public class ParallelService
{
    // Sequential - slow!
    public async Task<(User user, Order[] orders, decimal balance)> GetDataSequentialAsync(int userId)
    {
        var user = await GetUserAsync(userId);        // Wait...
        var orders = await GetOrdersAsync(userId);    // Then wait...
        var balance = await GetBalanceAsync(userId);  // Then wait...
        return (user, orders, balance);
        // Total time = user time + orders time + balance time
    }

    // Parallel - fast!
    public async Task<(User user, Order[] orders, decimal balance)> GetDataParallelAsync(int userId)
    {
        // Start all tasks at once
        var userTask = GetUserAsync(userId);
        var ordersTask = GetOrdersAsync(userId);
        var balanceTask = GetBalanceAsync(userId);

        // Wait for all to complete
        await Task.WhenAll(userTask, ordersTask, balanceTask);

        return (userTask.Result, ordersTask.Result, balanceTask.Result);
        // Total time = max(user time, orders time, balance time)
    }

    // Alternative syntax
    public async Task<(User, Order[], decimal)> GetDataParallel2Async(int userId)
    {
        var (user, orders, balance) = await (
            GetUserAsync(userId),
            GetOrdersAsync(userId),
            GetBalanceAsync(userId)
        );

        return (user, orders, balance);
    }
}
```

#### Handling Multiple Tasks

```csharp
// Wait for ALL tasks
public async Task ProcessAllAsync(List<int> ids)
{
    var tasks = ids.Select(id => ProcessAsync(id));
    await Task.WhenAll(tasks);  // Waits until ALL complete
}

// Wait for ANY task (first to complete)
public async Task<string> GetFastestResponseAsync(string[] urls)
{
    var tasks = urls.Select(url => FetchAsync(url));
    var firstResult = await Task.WhenAny(tasks);  // Returns when first completes
    return await firstResult;
}

// Process as they complete
public async Task ProcessAsCompletedAsync(List<int> ids)
{
    var tasks = ids.Select(id => ProcessAsync(id)).ToList();

    while (tasks.Any())
    {
        var completedTask = await Task.WhenAny(tasks);
        tasks.Remove(completedTask);

        var result = await completedTask;
        Console.WriteLine($"Completed: {result}");
    }
}
```

#### Cancellation

```csharp
public class CancellableService
{
    public async Task<string> LongOperationAsync(CancellationToken cancellationToken)
    {
        for (int i = 0; i < 100; i++)
        {
            // Check if cancellation requested
            cancellationToken.ThrowIfCancellationRequested();

            await Task.Delay(100, cancellationToken);  // Pass token to async operations
            Console.WriteLine($"Progress: {i}%");
        }

        return "Completed";
    }
}

// Usage
var cts = new CancellationTokenSource();
cts.CancelAfter(TimeSpan.FromSeconds(5));  // Auto-cancel after 5 seconds

try
{
    var result = await service.LongOperationAsync(cts.Token);
    Console.WriteLine(result);
}
catch (OperationCanceledException)
{
    Console.WriteLine("Operation was cancelled");
}

// Manual cancellation
var cts = new CancellationTokenSource();

// In UI: user clicks cancel button
cancelButton.Click += (s, e) => cts.Cancel();

// Operation checks token
await service.LongOperationAsync(cts.Token);
```

### Week 15: Async Best Practices

#### Common Mistakes

```csharp
// MISTAKE 1: Async void (except for event handlers)
public async void DoWorkAsync()  // BAD! Can't await, exceptions lost
{
    await Task.Delay(1000);
}

public async Task DoWorkAsync()  // GOOD!
{
    await Task.Delay(1000);
}

// MISTAKE 2: Blocking on async code
public string GetDataBlocking()
{
    var task = GetDataAsync();
    return task.Result;  // BAD! Can cause deadlock
}

public async Task<string> GetDataAsync()  // GOOD!
{
    return await GetDataInternalAsync();
}

// MISTAKE 3: Unnecessary async/await
public async Task<int> WrapperAsync()
{
    return await GetNumberAsync();  // Unnecessary wrapper
}

public Task<int> WrapperAsync()  // Better - return task directly
{
    return GetNumberAsync();
}

// But keep async/await for exception handling or using
public async Task<int> SafeWrapperAsync()
{
    try
    {
        return await GetNumberAsync();
    }
    catch (Exception ex)
    {
        // Exception properly caught with stack trace
        throw;
    }
}

// MISTAKE 4: Not using ConfigureAwait in library code
public async Task<string> LibraryMethodAsync()
{
    return await FetchDataAsync().ConfigureAwait(false);  // Don't capture context
}
```

#### ASP.NET Core Async

```csharp
// In ASP.NET Core, ALWAYS use async for I/O operations
public class AccountController : ControllerBase
{
    private readonly IAccountRepository _repository;

    // BAD - Blocks thread
    [HttpGet("{id}")]
    public ActionResult<Account> GetAccount(int id)
    {
        var account = _repository.GetByIdAsync(id).Result;  // BLOCKS!
        return Ok(account);
    }

    // GOOD - Async all the way
    [HttpGet("{id}")]
    public async Task<ActionResult<Account>> GetAccountAsync(int id)
    {
        var account = await _repository.GetByIdAsync(id);  // Non-blocking
        return Ok(account);
    }
}

// Repository should also be async
public class AccountRepository : IAccountRepository
{
    private readonly DbContext _context;

    public async Task<Account?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Accounts
            .Include(a => a.Transactions)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<List<Account>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Accounts
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Account account, CancellationToken cancellationToken = default)
    {
        await _context.Accounts.AddAsync(account, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
```

### Week 16: Advanced Async Scenarios

#### IAsyncEnumerable (Async Streams)

```csharp
// Stream data as it's available
public async IAsyncEnumerable<int> GenerateNumbersAsync(
    [EnumeratorCancellation] CancellationToken cancellationToken = default)
{
    for (int i = 0; i < 100; i++)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await Task.Delay(100, cancellationToken);
        yield return i;  // Return each number as it's generated
    }
}

// Usage
await foreach (var number in GenerateNumbersAsync())
{
    Console.WriteLine(number);  // Process as they arrive
}

// In ASP.NET Core controller
[HttpGet("stream")]
public async IAsyncEnumerable<Account> StreamAccountsAsync()
{
    await foreach (var account in _repository.GetAccountsStreamAsync())
    {
        yield return account;  // Streams to client
    }
}
```

---

## Phase 6: LINQ Mastery (Weeks 17-18)

### Week 17: LINQ Fundamentals

```csharp
var numbers = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
var people = new List<Person>
{
    new("Alice", 25, "IT"),
    new("Bob", 30, "HR"),
    new("Charlie", 35, "IT"),
    new("Diana", 28, "Finance"),
    new("Eve", 32, "IT")
};

// FILTERING
var evens = numbers.Where(n => n % 2 == 0);  // [2, 4, 6, 8, 10]
var itPeople = people.Where(p => p.Department == "IT");

// PROJECTION (Transform)
var doubled = numbers.Select(n => n * 2);  // [2, 4, 6, 8, 10, 12, 14, 16, 18, 20]
var names = people.Select(p => p.Name);  // ["Alice", "Bob", "Charlie", "Diana", "Eve"]
var summaries = people.Select(p => new { p.Name, p.Department });  // Anonymous type

// ORDERING
var sorted = numbers.OrderBy(n => n);  // Ascending
var sortedDesc = numbers.OrderByDescending(n => n);  // Descending
var sortedPeople = people.OrderBy(p => p.Department).ThenBy(p => p.Age);  // Multi-level sort

// AGGREGATION
var sum = numbers.Sum();  // 55
var average = numbers.Average();  // 5.5
var min = numbers.Min();  // 1
var max = numbers.Max();  // 10
var count = numbers.Count();  // 10
var countEven = numbers.Count(n => n % 2 == 0);  // 5

// ELEMENT OPERATIONS
var first = numbers.First();  // 1
var firstEven = numbers.First(n => n % 2 == 0);  // 2
var firstOrDefault = numbers.FirstOrDefault(n => n > 100);  // 0 (default for int)
var single = numbers.Single(n => n == 5);  // 5 (throws if not exactly one)
var elementAt = numbers.ElementAt(3);  // 4 (zero-indexed)

// QUANTIFIERS
var anyEven = numbers.Any(n => n % 2 == 0);  // true
var allPositive = numbers.All(n => n > 0);  // true
var contains = numbers.Contains(5);  // true

// SET OPERATIONS
var set1 = new[] { 1, 2, 3, 4 };
var set2 = new[] { 3, 4, 5, 6 };
var union = set1.Union(set2);  // [1, 2, 3, 4, 5, 6]
var intersect = set1.Intersect(set2);  // [3, 4]
var except = set1.Except(set2);  // [1, 2]
var distinct = new[] { 1, 1, 2, 2, 3 }.Distinct();  // [1, 2, 3]

// PARTITIONING
var firstThree = numbers.Take(3);  // [1, 2, 3]
var skipThree = numbers.Skip(3);  // [4, 5, 6, 7, 8, 9, 10]
var takeWhile = numbers.TakeWhile(n => n < 5);  // [1, 2, 3, 4]
var skipWhile = numbers.SkipWhile(n => n < 5);  // [5, 6, 7, 8, 9, 10]
var chunk = numbers.Chunk(3);  // [[1,2,3], [4,5,6], [7,8,9], [10]]

// GROUPING
var groupedByDept = people.GroupBy(p => p.Department);
foreach (var group in groupedByDept)
{
    Console.WriteLine($"{group.Key}: {string.Join(", ", group.Select(p => p.Name))}");
}
// IT: Alice, Charlie, Eve
// HR: Bob
// Finance: Diana

// JOIN
var departments = new[]
{
    new { Id = "IT", Name = "Information Technology" },
    new { Id = "HR", Name = "Human Resources" },
    new { Id = "Finance", Name = "Finance Department" }
};

var joined = people.Join(
    departments,
    person => person.Department,  // Key from people
    dept => dept.Id,              // Key from departments
    (person, dept) => new { person.Name, DepartmentName = dept.Name }
);
// [{ Name: "Alice", DepartmentName: "Information Technology" }, ...]

// CHAINING OPERATIONS
var result = numbers
    .Where(n => n > 3)      // [4, 5, 6, 7, 8, 9, 10]
    .Select(n => n * 2)     // [8, 10, 12, 14, 16, 18, 20]
    .Where(n => n < 18)     // [8, 10, 12, 14, 16]
    .OrderByDescending(n => n)  // [16, 14, 12, 10, 8]
    .Take(3)                // [16, 14, 12]
    .ToList();
```

### Week 18: Advanced LINQ

```csharp
// SELECTMANY - Flatten nested collections
var orders = new List<Order>
{
    new Order { Items = new[] { "Apple", "Banana" } },
    new Order { Items = new[] { "Orange", "Grape" } }
};
var allItems = orders.SelectMany(o => o.Items);  // ["Apple", "Banana", "Orange", "Grape"]

// AGGREGATE - Custom aggregation
var sentence = new[] { "Hello", "World", "!" };
var joined = sentence.Aggregate((a, b) => $"{a} {b}");  // "Hello World !"

var factorial = Enumerable.Range(1, 5).Aggregate((a, b) => a * b);  // 120

// ZIP - Combine two sequences
var names = new[] { "Alice", "Bob", "Charlie" };
var ages = new[] { 25, 30, 35 };
var combined = names.Zip(ages, (name, age) => $"{name} is {age}");
// ["Alice is 25", "Bob is 30", "Charlie is 35"]

// QUERY SYNTAX (alternative to method syntax)
var query = from p in people
            where p.Age > 25
            orderby p.Name
            select new { p.Name, p.Age };

// Equivalent method syntax
var method = people
    .Where(p => p.Age > 25)
    .OrderBy(p => p.Name)
    .Select(p => new { p.Name, p.Age });

// Complex query with join and group
var complexQuery = from p in people
                   join d in departments on p.Department equals d.Id
                   group p by d.Name into g
                   select new
                   {
                       Department = g.Key,
                       Count = g.Count(),
                       AverageAge = g.Average(p => p.Age)
                   };

// LINQ with Entity Framework (IQueryable)
public async Task<List<Account>> GetHighBalanceAccountsAsync(decimal threshold)
{
    return await _context.Accounts
        .Where(a => a.Balance > threshold)  // Translated to SQL
        .Include(a => a.Customer)
        .OrderByDescending(a => a.Balance)
        .Take(10)
        .ToListAsync();  // Executes query
}

// Deferred execution - query doesn't run until enumerated
var query = numbers.Where(n => n > 5);  // Not executed yet!
var list = query.ToList();  // NOW it executes

// Immediate execution
var count = numbers.Count(n => n > 5);  // Executes immediately
var first = numbers.First(n => n > 5);  // Executes immediately
var array = numbers.ToArray();  // Executes immediately
```

---

## Phase 7: Design Patterns & Architecture (Weeks 19-24)

(See COMPREHENSIVE_CODEBASE_GUIDE.md for detailed patterns already explained)

### Additional Patterns

#### Factory Pattern

```csharp
public interface IPaymentMethod
{
    void ProcessPayment(decimal amount);
}

public class CreditCard : IPaymentMethod
{
    public void ProcessPayment(decimal amount) => Console.WriteLine($"Credit card: {amount}");
}

public class PayPal : IPaymentMethod
{
    public void ProcessPayment(decimal amount) => Console.WriteLine($"PayPal: {amount}");
}

public class PaymentMethodFactory
{
    public IPaymentMethod Create(string type) => type switch
    {
        "creditcard" => new CreditCard(),
        "paypal" => new PayPal(),
        _ => throw new ArgumentException($"Unknown payment type: {type}")
    };
}
```

#### Builder Pattern

```csharp
public class Email
{
    public string From { get; set; }
    public string To { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
    public List<string> Attachments { get; set; } = new();
}

public class EmailBuilder
{
    private readonly Email _email = new();

    public EmailBuilder From(string from)
    {
        _email.From = from;
        return this;
    }

    public EmailBuilder To(string to)
    {
        _email.To = to;
        return this;
    }

    public EmailBuilder Subject(string subject)
    {
        _email.Subject = subject;
        return this;
    }

    public EmailBuilder Body(string body)
    {
        _email.Body = body;
        return this;
    }

    public EmailBuilder WithAttachment(string path)
    {
        _email.Attachments.Add(path);
        return this;
    }

    public Email Build() => _email;
}

// Usage
var email = new EmailBuilder()
    .From("sender@example.com")
    .To("recipient@example.com")
    .Subject("Hello")
    .Body("This is the message")
    .WithAttachment("file.pdf")
    .Build();
```

---

## Phase 8: Backend Development with ASP.NET Core (Weeks 25-32)

(Detailed in COMPREHENSIVE_CODEBASE_GUIDE.md)

### Quick Reference

```csharp
// Minimal API
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello World");
app.MapGet("/users/{id}", (int id) => $"User {id}");
app.MapPost("/users", (User user) => Results.Created($"/users/{user.Id}", user));

app.Run();

// Controller-based API
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetUser(int id)
    {
        var user = await _service.GetUserAsync(id);
        if (user == null) return NotFound();
        return Ok(user);
    }

    [HttpPost]
    public async Task<ActionResult<User>> CreateUser(CreateUserRequest request)
    {
        var user = await _service.CreateUserAsync(request);
        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
    }
}
```

---

## Mind Maps

### Mind Map 1: C# Type System

```
                            C# TYPE SYSTEM
                                  │
                    ┌─────────────┴─────────────┐
                    │                           │
              ┌─────▼─────┐               ┌─────▼─────┐
              │   VALUE   │               │ REFERENCE │
              │   TYPES   │               │   TYPES   │
              └─────┬─────┘               └─────┬─────┘
                    │                           │
        ┌───────────┼───────────┐      ┌────────┼────────┐
        │           │           │      │        │        │
   ┌────▼────┐ ┌────▼────┐ ┌────▼────┐ ┌───▼───┐ ┌───▼───┐
   │ NUMERIC │ │  BOOL   │ │  CHAR   │ │ CLASS │ │INTERFACE
   └────┬────┘ └─────────┘ └─────────┘ │       │ │       │
        │                              │string │ │ IList │
   ┌────┼────┬────┐                    │object │ │IDispose
   │    │    │    │                    │arrays │ └───────┘
  int float decimal                    └───────┘
  long double                               │
  byte                                ┌─────┼─────┐
  short                               │     │     │
                                    CLASS  ARRAY DELEGATE

              MEMORY
                │
        ┌───────┴───────┐
        │               │
   ┌────▼────┐     ┌────▼────┐
   │  STACK  │     │   HEAP  │
   │  (Fast) │     │ (Flexible)
   │         │     │         │
   │ Value   │     │Reference│
   │ Types   │     │  Types  │
   │ Local   │     │ Objects │
   │ vars    │     │ Strings │
   └─────────┘     └─────────┘
```

### Mind Map 2: Async/Await

```
                          ASYNC/AWAIT
                               │
              ┌────────────────┼────────────────┐
              │                │                │
        ┌─────▼─────┐    ┌─────▼─────┐    ┌─────▼─────┐
        │   TASK    │    │   ASYNC   │    │   AWAIT   │
        │ (Promise) │    │ (Modifier)│    │(Operator) │
        └─────┬─────┘    └─────┬─────┘    └─────┬─────┘
              │                │                │
         Represents       Marks method     Waits for
         async work       as async        Task completion
              │                │                │
        ┌─────┴─────┐    ┌─────┴─────┐    ┌─────┴─────┐
        │ Task<T>   │    │Return Task│    │Suspends   │
        │ Task      │    │ or Task<T>│    │execution  │
        │ ValueTask │    │           │    │           │
        └───────────┘    └───────────┘    └───────────┘

                    PATTERNS
                       │
        ┌──────────────┼──────────────┐
        │              │              │
   ┌────▼────┐   ┌─────▼─────┐   ┌────▼────┐
   │  AWAIT  │   │ WhenAll   │   │ WhenAny │
   │  EACH   │   │ (Parallel)│   │  (Race) │
   └─────────┘   └───────────┘   └─────────┘

                CANCELLATION
                     │
              ┌──────┴──────┐
              │             │
         ┌────▼────┐   ┌────▼────┐
         │  Token  │   │ Source  │
         │(Checker)│   │(Creator)│
         └─────────┘   └─────────┘
```

### Mind Map 3: SOLID Principles

```
                           S.O.L.I.D.
                               │
        ┌──────────────────────┼──────────────────────┐
        │                      │                      │
   ┌────▼────┐           ┌─────▼─────┐          ┌────▼────┐
   │    S    │           │     O     │          │    L    │
   │ Single  │           │Open/Close │          │ Liskov  │
   │Responsibility       │           │          │Substitution
   └────┬────┘           └─────┬─────┘          └────┬────┘
        │                      │                      │
   One class            Open for           Subtypes must be
   one reason           extension          substitutable for
   to change            closed for         base types
                        modification

        ┌──────────────────────┼──────────────────────┐
        │                      │                      │
   ┌────▼────┐           ┌─────▼─────┐
   │    I    │           │     D     │
   │Interface│           │Dependency │
   │Segregation          │ Inversion │
   └────┬────┘           └─────┬─────┘
        │                      │
   Many specific         Depend on
   interfaces better     abstractions
   than one fat one      not concretions
```

### Mind Map 4: C# Learning Path

```
                     C# MASTERY PATH
                           │
        ┌──────────────────┼──────────────────┐
        │                  │                  │
   ┌────▼────┐       ┌─────▼─────┐      ┌────▼────┐
   │BEGINNER │       │INTERMEDIATE       │ADVANCED │
   │(1-2 mo) │       │  (3-4 mo)  │      │(5-8 mo) │
   └────┬────┘       └─────┬─────┘      └────┬────┘
        │                  │                  │
   ┌────┴────┐       ┌─────┴─────┐      ┌────┴────┐
   │Variables│       │  Generics │      │Patterns │
   │Controls │       │   LINQ    │      │   DDD   │
   │  OOP    │       │Async/Await│      │  CQRS   │
   │ Classes │       │Interfaces │      │ASP.NET  │
   └─────────┘       └───────────┘      └─────────┘

                    PRACTICE
                       │
        ┌──────────────┼──────────────┐
        │              │              │
   ┌────▼────┐   ┌─────▼─────┐   ┌────▼────┐
   │  Small  │   │  Medium   │   │  Large  │
   │Projects │   │  Projects │   │Projects │
   │(Console)│   │   (API)   │   │  (Full) │
   └─────────┘   └───────────┘   └─────────┘
```

---

## Practice Projects

### Beginner Projects (Weeks 1-8)

1. **Calculator Console App**
   - Basic operations
   - Input validation
   - Menu system

2. **Todo List Manager**
   - Add/Remove items
   - Mark complete
   - Save to file
   - Use classes

3. **Bank Account Simulator**
   - Create accounts
   - Deposit/Withdraw
   - Transfer money
   - Transaction history

### Intermediate Projects (Weeks 9-18)

4. **Library Management System**
   - Book/Member entities
   - Borrow/Return operations
   - Generic repository
   - LINQ queries
   - Async file operations

5. **REST API for Inventory**
   - CRUD operations
   - Entity Framework Core
   - Validation
   - Error handling
   - Result pattern

6. **Event-Driven Order System**
   - Domain events
   - Event handlers
   - Async processing
   - Order workflow

### Advanced Projects (Weeks 19-32)

7. **E-Commerce Backend**
   - Clean Architecture
   - CQRS pattern
   - Domain-Driven Design
   - MediatR pipeline
   - SignalR notifications

8. **Microservices Demo**
   - Multiple services
   - Message queue
   - Service discovery
   - Resilience patterns
   - Docker containers

---

## Feedback & Self-Assessment

### Weekly Self-Check Questions

**Week 1-4 (Fundamentals)**
- [ ] Can I explain the difference between value and reference types?
- [ ] Do I understand when to use `int` vs `decimal` vs `double`?
- [ ] Can I write a method with default parameters?
- [ ] Do I understand `ref` and `out` parameters?
- [ ] Can I choose the right collection for a problem?

**Week 5-8 (OOP)**
- [ ] Can I design a class hierarchy with inheritance?
- [ ] Do I understand when to use `virtual` vs `abstract`?
- [ ] Can I explain the difference between interface and abstract class?
- [ ] Do I follow SOLID principles in my code?
- [ ] Can I identify code smells and refactor them?

**Week 9-12 (Advanced C#)**
- [ ] Can I use records for immutable data?
- [ ] Do I understand pattern matching and when to use it?
- [ ] Can I write custom events and handle them?
- [ ] Do I know how to create and use custom exceptions?
- [ ] Can I explain delegates and lambda expressions?

**Week 13-16 (Async)**
- [ ] Can I convert synchronous code to asynchronous?
- [ ] Do I understand when to use `Task.WhenAll` vs `Task.WhenAny`?
- [ ] Can I implement cancellation properly?
- [ ] Do I avoid common async pitfalls (async void, blocking)?
- [ ] Can I use `IAsyncEnumerable` for streaming?

**Week 17-18 (LINQ)**
- [ ] Can I chain LINQ methods effectively?
- [ ] Do I understand deferred vs immediate execution?
- [ ] Can I write complex queries with joins and groups?
- [ ] Do I know when to use method vs query syntax?
- [ ] Can I optimize LINQ queries for performance?

**Week 19-32 (Architecture)**
- [ ] Can I implement the Repository pattern?
- [ ] Do I understand Clean Architecture layers?
- [ ] Can I use MediatR for CQRS?
- [ ] Do I know how to implement domain events?
- [ ] Can I build a complete ASP.NET Core API?

### Code Review Checklist

Before submitting any code, check:

```
[ ] Code compiles without warnings
[ ] All methods have clear, descriptive names
[ ] Complex logic has comments explaining WHY
[ ] No magic numbers (use constants)
[ ] Proper exception handling
[ ] Input validation present
[ ] Resources disposed properly (using statements)
[ ] Async methods are truly async (no blocking)
[ ] LINQ queries are efficient
[ ] SOLID principles followed
[ ] Unit tests written
[ ] No copy-paste code (DRY principle)
```

### Performance Benchmarking

Track your progress:

```csharp
// How long does it take you to:

// Level 1: Write a class with properties, constructor, methods
// Target: < 5 minutes

// Level 2: Implement interface with multiple methods
// Target: < 10 minutes

// Level 3: Write async method with proper error handling
// Target: < 15 minutes

// Level 4: Create LINQ query with filtering, grouping, projection
// Target: < 10 minutes

// Level 5: Implement full CRUD API endpoint
// Target: < 30 minutes

// Level 6: Design domain entity with validation and events
// Target: < 20 minutes
```

---

## Resources & Learning Path

### Books (In Order)
1. "C# in Depth" by Jon Skeet
2. "CLR via C#" by Jeffrey Richter
3. "Clean Code" by Robert C. Martin
4. "Domain-Driven Design" by Eric Evans
5. "Designing Data-Intensive Applications" by Martin Kleppmann

### Online Resources
- Microsoft Learn (free, official)
- Pluralsight (paid, comprehensive)
- YouTube: Nick Chapsas, Tim Corey, IAmTimCorey
- GitHub: Explore open-source C# projects

### Practice Platforms
- LeetCode (algorithms)
- HackerRank (C# challenges)
- Exercism (mentored learning)
- Codewars (kata challenges)

### Community
- Stack Overflow
- Reddit r/csharp
- Discord C# servers
- Local .NET user groups

---

## Final Advice

### The 10,000 Hour Rule Applied to C#

```
Hours 0-100:      Syntax, basics, "Hello World"
Hours 100-500:    OOP, collections, basic patterns
Hours 500-1000:   Async, LINQ, error handling
Hours 1000-2500:  Architecture, design patterns
Hours 2500-5000:  Production systems, performance
Hours 5000-10000: Mastery, teaching others, innovation
```

### Daily Practice Routine

```
Morning (30 min):
- Read one C# article/blog post
- Watch one short tutorial video

Afternoon (1-2 hours):
- Work on practice project
- Write actual code
- Solve one coding challenge

Evening (30 min):
- Review what you learned
- Document new concepts
- Plan tomorrow's learning
```

### The Growth Mindset

```
DON'T SAY                          SAY INSTEAD
───────────────────────────────────────────────
"I don't understand generics"  →  "I don't understand generics YET"
"This is too hard"             →  "This will take more practice"
"I made a mistake"             →  "I learned what doesn't work"
"I'm not a backend developer"  →  "I'm becoming a backend developer"
```

Remember: Every expert was once a beginner. The key is consistent practice and never stopping your learning journey.

**You've got this!**

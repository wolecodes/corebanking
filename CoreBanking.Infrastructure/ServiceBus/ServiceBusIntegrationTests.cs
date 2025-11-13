// using CoreBanking.Core.Events;
// using CoreBanking.Core.ValueObjects;
// namespace CoreBanking.Infrastructure.ServiceBus;


// public class ServiceBusIntegrationTests : IAsyncLifetime
// {
//   private readonly ServiceBusAdministration _admin;
//   private readonly ServiceBusEventPublisher _publisher;
//   private readonly CustomerEventHandler _customerHandler;
//   private readonly TestNotificationService _testNotificationService;

//   public async Task InitializeAsync()
//   {
//     await _admin.EnsureInfrastructureExistsAsync();
//   }

//   public async Task DisposeAsync()
//   {
//     // Cleanup test resources
//   }

//   [Fact]
//   public async Task PublishCustomerCreatedEvent_ShouldBeReceivedByHandler()
//   {
//     // Arrange
//     var customerEvent = new CustomerCreatedEvent(
//         CustomerId.Create(Guid.NewGuid()),
//         "John",
//         "Doe",
//         "john.doe@example.com",
//         "12345678901",
//         DateTime.UtcNow.AddYears(-30));

//     // Act
//     await _publisher.PublishAsync(customerEvent);

//     // Wait for processing
//     await Task.Delay(TimeSpan.FromSeconds(5));

//     // Assert
//     Assert.True(_testNotificationService.WelcomeEmailsSent.Contains(customerEvent.Email));
//     Assert.True(_testNotificationService.WelcomeEmailsSentCount == 1);
//   }

//   [Fact]
//   public async Task PublishMultipleEvents_ShouldUseBatchProcessing()
//   {
//     // Arrange
//     var events = new List<CustomerCreatedEvent>();
//     for (int i = 0; i < 15; i++)
//     {
//       events.Add(new CustomerCreatedEvent(
//           CustomerId.Create(Guid.NewGuid()),
//           $"User{i}",
//           "Test",
//           $"user{i}@test.com",
//           $"2000000000{i}",
//           DateTime.UtcNow.AddYears(-25)));
//     }

//     // Act
//     await _publisher.PublishBatchAsync(events);

//     // Wait for processing
//     await Task.Delay(TimeSpan.FromSeconds(10));

//     // Assert
//     Assert.Equal(15, _testNotificationService.WelcomeEmailsSentCount);
//   }
// }

// // Test double for notification service
// public class TestNotificationService : INotificationService
// {
//   public List<string> WelcomeEmailsSent { get; } = new();
//   public int WelcomeEmailsSentCount => WelcomeEmailsSent.Count;

//   public Task SendWelcomeEmailAsync(string email, string firstName, CancellationToken cancellationToken)
//   {
//     WelcomeEmailsSent.Add(email);
//     return Task.CompletedTask;
//   }

//   // Implement other interface methods...
// } 
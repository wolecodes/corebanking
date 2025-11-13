using CoreBanking.Core.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CoreBanking.Infrastructure.ServiceBus.Handlers
{
  public class CustomerEventServiceBusHandler : BaseMessageHandler<CustomerCreatedEvent>
  {
    public CustomerEventServiceBusHandler(
        IServiceBusClientFactory clientFactory,
        ServiceBusConfiguration config,
        ILogger<CustomerEventServiceBusHandler> logger,
        IMediator mediator)
        : base(clientFactory, config.CustomerTopicName, "notifications", logger, mediator)
    {
    }
    // No need to override HandleMessageAsync - base class uses MediatR
  }
}

using CoreBanking.Core.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CoreBanking.Infrastructure.ServiceBus.Handlers
{
  public class TransactionEventServiceBusHandler : BaseMessageHandler<MoneyTransferedEvent>
  {
    public TransactionEventServiceBusHandler(
        IServiceBusClientFactory clientFactory,
        ServiceBusConfiguration config,
        ILogger<TransactionEventServiceBusHandler> logger,
        IMediator mediator)
        : base(clientFactory, config.TransactionTopicName, "fraud-detection", logger, mediator)
    {
    }
  }
}

namespace CoreBanking.Core.Interfaces;

public interface IDomainEvent
{
    DateTime OccurredOn { get; }
}


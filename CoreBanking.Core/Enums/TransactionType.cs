using System;

namespace CoreBanking.Core.Enums;

public enum TransactionType
{
    Deposit = 1,
    Withdrawal = 2,
    Transfer = 3,
    Interest = 4,
    TransferIn = 5,
    TransferOut = 6
}

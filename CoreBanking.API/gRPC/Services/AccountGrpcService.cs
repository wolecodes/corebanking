using AutoMapper;
using CoreBanking.APP.Accounts.Commands.CreateAccount;
using CoreBanking.APP.Accounts.Commands.TransferMoney;
using CoreBanking.APP.Accounts.Queries.GetAccountDetails;
using CoreBanking.APP.Accounts.Queries.GetTransactionHistory;
using CoreBanking.Core.ValueObjects;
using Grpc.Core;
using MediatR;
namespace CoreBanking.API.gRPC;

public class AccountGrpcService : AccountService.AccountServiceBase
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;
    private readonly ILogger<AccountGrpcService> _logger;

    public AccountGrpcService(IMediator mediator, IMapper mapper, ILogger<AccountGrpcService> logger)
    {
        _mediator = mediator;
        _mapper = mapper;
        _logger = logger;
    }

    public override async Task<AccountResponse> GetAccount(GetAccountRequest request,
         ServerCallContext context)
    {
        _logger.LogInformation("gRPC GetAccount called for {AccountNumber}", request.AccountNumber);

        // Validate input
        if (string.IsNullOrWhiteSpace(request.AccountNumber))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Account number is required"));

        try
        {
            var accountNumber = AccountNumber.Create(request.AccountNumber);
            var query = new GetAccountDetailsQuery { AccountNumber = accountNumber };
            var result = await _mediator.Send(query);

            return result.Match<AccountResponse>(
                success: account => _mapper.Map<AccountResponse>(account),
                failure: errors => throw new RpcException(new Status(StatusCode.NotFound,
                    string.Join("; ", errors)))
            );
        }
        catch (Exception ex) when (ex is not RpcException)
        {
            _logger.LogError(ex, "Error retrieving account {AccountNumber}", request.AccountNumber);
            throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
        }
    }

    public override async Task<CreateAccountResponse> CreateAccount(CreateAccountRequest request,
        ServerCallContext context)
    {
        _logger.LogInformation("gRPC CreateAccount called for customer {CustomerId}", request.CustomerId);

        if (!Guid.TryParse(request.CustomerId, out var customerGuid))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid customer ID format"));

        try
        {
            var command = new CreateAccountCommand
            {
                CustomerId = CustomerId.Create(customerGuid),
                AccountType = request.AccountType,
                InitialDeposit = (decimal)request.InitialDeposit,
                Currency = request.Currency
            };

            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument,
                    string.Join("; ", result.Errors)));
            }

            return new CreateAccountResponse
            {
                AccountId = result.Data.ToString(),
                AccountNumber = "Temp",// Would come from created account
                Message = "Account created successfully"
            };
        }
        catch (Exception ex) when (ex is not RpcException)
        {
            _logger.LogError(ex, "Error creating account for customer {CustomerId}", request.CustomerId);
            throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
        }
    }
    public override async Task<TransferMoneyResponse> TransferMoney(TransferMoneyRequest request,
        ServerCallContext context)
    {
        var command = new TransferMoneyCommand
        {
            SourceAccountNumber = AccountNumber.Create(request.SourceAccountNumber),
            DestinationAccountNumber = AccountNumber.Create(request.DestinationAccountNumber),
            Amount = new Core.ValueObjects.Money((decimal)request.Amount, request.Currency),
            Reference = request.Reference,
            Description = request.Description
        };

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            var status = result.Errors.Any(e => e.Contains("insufficient", StringComparison.OrdinalIgnoreCase))
                ? StatusCode.FailedPrecondition
                : StatusCode.InvalidArgument;

            throw new RpcException(new Status(status, string.Join(", ", result.Errors)));
        }

        return new TransferMoneyResponse
        {
            Success = true,
            Message = "Transfer completed successfully",
            TransferDate = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow)
        };
    }

    public override async Task GetTransactionHistory(TransactionHistoryRequest request,
        IServerStreamWriter<TransactionResponse> responseStream, ServerCallContext context)
    {
        var query = new GetTransactionHistoryQuery
        {
            AccountNumber = AccountNumber.Create(request.AccountNumber),
            StartDate = request.StartDate?.ToDateTime(),
            EndDate = request.EndDate?.ToDateTime(),
            PageSize = request.PageSize
        };

        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
            throw new RpcException(new Status(StatusCode.NotFound, string.Join(", ", result.Errors)));

        foreach (var transaction in result.Data!.Transactions)
        {
            if (context.CancellationToken.IsCancellationRequested)
                break;

            await responseStream.WriteAsync(_mapper.Map<TransactionResponse>(transaction));
            await Task.Delay(100); // Simulate processing time
        }
    }
}
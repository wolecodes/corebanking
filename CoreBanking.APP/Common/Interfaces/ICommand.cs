using CoreBanking.APP.Common.Models;
using MediatR;

namespace CoreBanking.APP.Common.Interfaces;

public interface ICommand : IRequest<Result> { }
public interface ICommand<TResponse> : IRequest<Result<TResponse>> { }
public interface IQuery<TResponse> : IRequest<Result<TResponse>> { }

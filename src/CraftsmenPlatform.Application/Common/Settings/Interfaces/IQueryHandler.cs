using CraftsmenPlatform.Domain.Common;

namespace CraftsmenPlatform.Application.Common.Settings.Interfaces;

public interface IQueryHandler<in TQuery, TResponse> where TQuery : IQuery<TResponse>
{
    Task<Result<TResponse>> Handle(TQuery query, CancellationToken cancellationToken);
}
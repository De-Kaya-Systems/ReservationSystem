using DeKayaServer.Application.Behaviors;
using DeKayaServer.Contracts.CustomerAccount;
using TS.MediatR;
using TS.Result;

namespace DeKayaServer.Application.Customers;

[Permission( "customer-account:get" )]
public sealed record CustomerAccountGetQuery(
    Guid CustomerId,
    DateTime? StartDate,
    DateTime? EndDate )
    : IRequest<Result<CustomerAccountDto>>;

internal sealed class CustomerAccountGetQueryHandler(
    ICustomerAccountReaderService customerAccountReaderService )
    : IRequestHandler<CustomerAccountGetQuery, Result<CustomerAccountDto>>
{
    public Task<Result<CustomerAccountDto>> Handle(
        CustomerAccountGetQuery request,
        CancellationToken cancellationToken )
        => customerAccountReaderService.GetAccountAsync(
            customerId: request.CustomerId,
            startDate: request.StartDate,
            endDate: request.EndDate,
            cancellationToken: cancellationToken );
}

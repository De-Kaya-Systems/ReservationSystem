using DeKayaServer.Application.Behaviors;
using DeKayaServer.Contracts.Customers;
using DeKayaServer.Domain.Customers;
using TS.MediatR;

namespace DeKayaServer.Application.Customers;

[Permission( "customer:getall" )]
public sealed record CustomerGetAllQuery : IRequest<IQueryable<CustomerDto>>;

internal sealed class CustomerGetAllQueryHandler(
    ICustomerRepository customerRepository ) : IRequestHandler<CustomerGetAllQuery, IQueryable<CustomerDto>>
{
    public Task<IQueryable<CustomerDto>> Handle( CustomerGetAllQuery request, CancellationToken cancellationToken )
    {
        var query = customerRepository
            .GetAllWithAudit()
            .MapTo()
            .AsQueryable();
        return Task.FromResult( query );
    }
}

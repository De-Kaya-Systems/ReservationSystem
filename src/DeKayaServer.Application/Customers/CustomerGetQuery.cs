using DeKayaServer.Application.Behaviors;
using DeKayaServer.Contracts.Customers;
using DeKayaServer.Domain.Customers;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace DeKayaServer.Application.Customers;

[Permission( "customer:get" )]
public sealed record CustomerGetQuery( Guid Id ) : IRequest<Result<CustomerDto>>;

internal sealed class CustomerGetQueryHandler(
    ICustomerRepository customerRepository ) : IRequestHandler<CustomerGetQuery, Result<CustomerDto>>
{
    public async Task<Result<CustomerDto>> Handle( CustomerGetQuery request, CancellationToken cancellationToken )
    {
        var res = await customerRepository
             .GetAllWithAudit()
             .MapTo()
             .Where( c => c.Id == request.Id )
             .FirstOrDefaultAsync( cancellationToken );

        if ( res is null )
        {
            return Result<CustomerDto>.Failure( "Customer not found" );
        }

        return res;
    }
}

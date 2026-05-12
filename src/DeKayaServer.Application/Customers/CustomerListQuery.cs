using DeKayaServer.Application.Behaviors;
using DeKayaServer.Contracts.Common;
using DeKayaServer.Contracts.Customers;
using DeKayaServer.Domain.Customers;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace DeKayaServer.Application.Customers;

[Permission( "customer:getall" )]
public sealed record CustomerListQuery(
    string? CustomerName,
    string? PhoneNumber,
    int PageIndex,
    int PageSize )
    : IRequest<Result<PagedResultDto<CustomerListItemDto>>>;

internal sealed class CustomerListQueryHandler(
    ICustomerRepository customerRepository )
    : IRequestHandler<CustomerListQuery, Result<PagedResultDto<CustomerListItemDto>>>
{
    public async Task<Result<PagedResultDto<CustomerListItemDto>>> Handle(
        CustomerListQuery request,
        CancellationToken cancellationToken )
    {
        var pageIndex = Math.Max( request.PageIndex, 0 );
        var pageSize = Math.Clamp( request.PageSize <= 0 ? 25 : request.PageSize, 1, 100 );

        var query = customerRepository.GetAllWithAudit();

        if ( !string.IsNullOrWhiteSpace( request.CustomerName ) )
        {
            var customerName = request.CustomerName.Trim();

            query = query.Where( x =>
                EF.Functions.Like( x.Entity.FullName.Value, $"%{customerName}%" ) );
        }

        if ( !string.IsNullOrWhiteSpace( request.PhoneNumber ) )
        {
            var phoneNumber = request.PhoneNumber.Trim();

            query = query.Where( x =>
                EF.Functions.Like( x.Entity.Contact.PhoneNumber, $"%{phoneNumber}%" )
                || ( x.Entity.Contact.PhoneNumber2 != null
                     && EF.Functions.Like( x.Entity.Contact.PhoneNumber2, $"%{phoneNumber}%" ) ) );
        }

        var totalCount = await query.CountAsync( cancellationToken );

        var items = await query
            .OrderBy( x => x.Entity.FullName.Value )
            .ThenBy( x => x.Entity.Contact.PhoneNumber )
            .Skip( pageIndex * pageSize )
            .Take( pageSize )
            .Select( x => new CustomerListItemDto
            {
                Id = x.Entity.Id,
                FullName = x.Entity.FullName.Value,
                PhoneNumber = x.Entity.Contact.PhoneNumber,
                PhoneNumber2 = x.Entity.Contact.PhoneNumber2,
                Email = x.Entity.Contact.Email,

                City = x.Entity.Address.City,
                District = x.Entity.Address.District,
                FullAddress = x.Entity.Address.FullAddress,

                IsActive = x.Entity.IsActive,
                CreatedAt = x.Entity.CreatedAt,
                CreatedFullName = x.CreatedUser.FullName.Value,
                UpdatedAt = x.Entity.UpdatedAt,
                UpdatedFullName = x.UpdatedUser != null
                    ? x.UpdatedUser.FullName.Value
                    : null
            } )
            .ToListAsync( cancellationToken );

        return Result<PagedResultDto<CustomerListItemDto>>.Succeed( new()
        {
            Items = items,
            TotalCount = totalCount,
            PageIndex = pageIndex,
            PageSize = pageSize,
            TotalPages = totalCount == 0
                ? 0
                : ( int )Math.Ceiling( totalCount / ( double )pageSize )
        } );
    }
}

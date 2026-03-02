using DeKayaServer.Contracts.Customers;
using DeKayaServer.Domain.Abstractions;
using DeKayaServer.Domain.Customers;

namespace DeKayaServer.Application.Customers;

public static class CustomerMappingExtensions
{
    public static IQueryable<CustomerDto> MapTo(
        this IQueryable<EntityWithAuditDto<Customer>> entities )
    {
        var res = entities
            .Select( x => new CustomerDto
            {
                Id = x.Entity.Id,
                IsActive = x.Entity.IsActive,

                FirstName = x.Entity.FirstName.Value,
                LastName = x.Entity.LastName.Value,
                FullName = x.Entity.FullName.Value,

                City = x.Entity.Address.City,
                District = x.Entity.Address.District,
                FullAddress = x.Entity.Address.FullAddress,

                PhoneNumber = x.Entity.Contact.PhoneNumber,
                PhoneNumber2 = x.Entity.Contact.PhoneNumber2,
                Email = x.Entity.Contact.Email,

                CreatedAt = x.Entity.CreatedAt,
                CreatedBy = x.CreatedUser.CreatedBy,
                CreatedFullName = x.CreatedUser.FullName.Value,
                UpdatedAt = x.Entity.UpdatedAt,
                UpdatedBy = x.Entity.UpdatedBy != null ? x.Entity.UpdatedBy.Value : null,
                UpdatedFullName = x.UpdatedUser != null ? x.UpdatedUser.FullName.Value : null
            } );
        return res;
    }
}

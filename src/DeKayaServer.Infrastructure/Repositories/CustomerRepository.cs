using DeKayaServer.Domain.Customers;
using DeKayaServer.Infrastructure.Abstractions;
using DeKayaServer.Infrastructure.Context;

namespace DeKayaServer.Infrastructure.Repositories;

internal sealed class CustomerRepository( ApplicationDbContext context ) : AuditableRepository<Customer, ApplicationDbContext>( context ), ICustomerRepository
{
}

using DeKayaServer.Domain.CustomerBalance;
using DeKayaServer.Infrastructure.Abstractions;
using DeKayaServer.Infrastructure.Context;

namespace DeKayaServer.Infrastructure.Repositories;

internal sealed class CustomerBalanceRepository( ApplicationDbContext context ) : AuditableRepository<CustomerBalance, ApplicationDbContext>( context ), ICustomerBalanceRepository
{
}
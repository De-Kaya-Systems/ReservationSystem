using DeKayaServer.Domain.CustomerAccountAdjustments;
using DeKayaServer.Infrastructure.Abstractions;
using DeKayaServer.Infrastructure.Context;

namespace DeKayaServer.Infrastructure.Repositories;

internal sealed class CustomerAccountAdjustmentRepository( ApplicationDbContext context ) : AuditableRepository<CustomerAccountAdjustment, ApplicationDbContext>( context ), ICustomerAccountAdjustmentRepository
{
}

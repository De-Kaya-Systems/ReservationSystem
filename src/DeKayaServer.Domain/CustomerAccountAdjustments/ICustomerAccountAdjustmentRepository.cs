using DeKayaServer.Domain.Abstractions;

namespace DeKayaServer.Domain.CustomerAccountAdjustments;

public interface ICustomerAccountAdjustmentRepository : IAuditableRepository<CustomerAccountAdjustment>
{
}
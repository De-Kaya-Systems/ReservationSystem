using DeKayaServer.Domain.Abstractions;

namespace DeKayaServer.Domain.CustomerBalance;

public interface ICustomerBalanceRepository : IAuditableRepository<CustomerBalance>
{
}
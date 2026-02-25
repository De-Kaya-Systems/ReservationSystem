using DeKayaServer.Domain.Abstractions;

namespace DeKayaServer.Domain.Customers;

public interface ICustomerRepository : IAuditableRepository<Customer>
{
}

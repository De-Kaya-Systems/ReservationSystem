using DeKayaServer.Contracts.CustomerAccount;
using TS.Result;

namespace DeKayaServer.Application.Customers;

internal interface ICustomerAccountReaderService
{
    Task<Result<CustomerAccountDto>> GetAccountAsync(
        Guid customerId,
        DateTime? startDate,
        DateTime? endDate,
        CancellationToken cancellationToken = default );
}

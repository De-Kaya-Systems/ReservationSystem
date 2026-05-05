using DeKayaServer.Domain.Abstractions;

namespace DeKayaServer.Domain.PaymentHistory;

public interface IPaymentHistoryRepository : IAuditableRepository<PaymentHistory>
{
}

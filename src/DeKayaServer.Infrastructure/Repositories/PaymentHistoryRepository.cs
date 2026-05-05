using DeKayaServer.Domain.PaymentHistory;
using DeKayaServer.Infrastructure.Abstractions;
using DeKayaServer.Infrastructure.Context;

namespace DeKayaServer.Infrastructure.Repositories;

internal sealed class PaymentHistoryRepository( ApplicationDbContext context ) : AuditableRepository<PaymentHistory, ApplicationDbContext>( context ), IPaymentHistoryRepository
{
}

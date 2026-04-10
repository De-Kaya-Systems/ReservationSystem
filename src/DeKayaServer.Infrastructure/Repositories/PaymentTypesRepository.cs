using DeKayaServer.Domain.PaymentTypes;
using DeKayaServer.Infrastructure.Abstractions;
using DeKayaServer.Infrastructure.Context;

namespace DeKayaServer.Infrastructure.Repositories;

internal sealed class PaymentTypesRepository( ApplicationDbContext context ) : AuditableRepository<PaymentTypes, ApplicationDbContext>( context ), IPaymentTypesRepository
{
}
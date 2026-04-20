using DeKayaServer.Application.Behaviors;
using DeKayaServer.Contracts.PaymentTypes;
using DeKayaServer.Domain.PaymentTypes;
using TS.MediatR;

namespace DeKayaServer.Application.PaymentTypes;

[Permission( "paymenttype:getall" )]
public sealed record PaymentTypeGetAllQuery : IRequest<IQueryable<PaymentTypeDto>>;

internal sealed class PaymentTypeGetAllQueryHandler(
    IPaymentTypesRepository paymentTypesRepository ) : IRequestHandler<PaymentTypeGetAllQuery, IQueryable<PaymentTypeDto>>
{
    public Task<IQueryable<PaymentTypeDto>> Handle( PaymentTypeGetAllQuery request, CancellationToken cancellationToken )
    {
        var query = paymentTypesRepository
            .GetAllWithAudit()
            .MapToDto()
            .AsQueryable();

        return Task.FromResult( query );
    }
}
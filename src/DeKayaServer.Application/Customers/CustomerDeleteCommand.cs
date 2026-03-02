using DeKayaServer.Application.Behaviors;
using DeKayaServer.Domain.Customers;
using GenericRepository;
using TS.MediatR;
using TS.Result;

namespace DeKayaServer.Application.Customers;

[Permission( "customer:delete" )]
public sealed record CustomerDeleteCommand(
    Guid Id ) : IRequest<Result<string>>;

internal sealed class CustomerDeleteCommandHandler(
    ICustomerRepository customerRepository,
    IUnitOfWork unitOfWork ) : IRequestHandler<CustomerDeleteCommand, Result<string>>
{
    public async Task<Result<string>> Handle( CustomerDeleteCommand request, CancellationToken cancellationToken )
    {
        var customer = await customerRepository.FirstOrDefaultAsync( x => x.Id == request.Id, cancellationToken );
        if ( customer is null )
        {
            return Result<string>.Failure( "Müşteri bulunamadı" );
        }

        customer.Delete();
        await unitOfWork.SaveChangesAsync( cancellationToken );
        return "Müşteri başarıyla silindi";
    }
}
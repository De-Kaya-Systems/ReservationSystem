using DeKayaServer.Application.Behaviors;
using DeKayaServer.Domain.Customers;
using DeKayaServer.Domain.Customers.ValueObjects;
using DeKayaServer.Domain.Shared;
using FluentValidation;
using GenericRepository;
using TS.MediatR;
using TS.Result;

namespace DeKayaServer.Application.Customers;

[Permission( "customer:edit" )]
public sealed record CustomerUpdateCommand(
    Guid Id,
    string FirstName,
    string LastName,
    Address Address,
    Contact Contact,
    bool isActive
     ) : IRequest<Result<string>>;

public sealed class CustomerUpdateCommandValidator : AbstractValidator<CustomerUpdateCommand>
{
    public CustomerUpdateCommandValidator()
    {
        RuleFor( x => x.FirstName )
            .NotEmpty()
            .WithMessage( "Geçerli bir müşteri adı girin" );
        RuleFor( x => x.LastName )
            .NotEmpty()
            .WithMessage( "Geçerli bir müşteri soyadı girin" );
        RuleFor( x => x.Address.City )
            .NotEmpty()
            .WithMessage( "Geçerli bir şehir girin" );
        RuleFor( x => x.Address.District )
            .NotEmpty()
            .WithMessage( "Geçerli bir ilçe girin" );
        RuleFor( x => x.Address.FullAddress )
            .NotEmpty()
            .WithMessage( "Geçerli bir adres girin" );
        RuleFor( x => x.Contact.PhoneNumber )
            .NotEmpty()
            .WithMessage( "Geçerli bir telefon numarası yazın" );
    }
}

internal sealed class UserUpdateCommandHandler(
    ICustomerRepository customerRepository,
    IUnitOfWork unitOfWork ) : IRequestHandler<CustomerUpdateCommand, Result<string>>
{
    public async Task<Result<string>> Handle( CustomerUpdateCommand request, CancellationToken cancellationToken )
    {
        var customer = await customerRepository.FirstOrDefaultAsync( x => x.Id == request.Id, cancellationToken );
        if ( customer is null )
        {
            return Result<string>.Failure( "Müşteri bulunamadı" );
        }

        FirstName firstName = new( request.FirstName );
        LastName lastName = new( request.LastName );
        Address address = request.Address;
        Contact contact = request.Contact;

        customer.SetFirstName( firstName );
        customer.SetLastName( lastName );
        customer.SetAddress( address );
        customer.SetContact( contact );
        customer.SetStatus( request.isActive );
        customer.SetFullName();

        customerRepository.Update( customer );
        await unitOfWork.SaveChangesAsync( cancellationToken );
        return "Müşteri başarıyla güncellendi";
    }
}
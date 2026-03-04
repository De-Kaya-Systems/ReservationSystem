using DeKayaServer.Application.Behaviors;
using DeKayaServer.Domain.Customers;
using DeKayaServer.Domain.Customers.ValueObjects;
using DeKayaServer.Domain.Shared;
using FluentValidation;
using GenericRepository;
using TS.MediatR;
using TS.Result;

namespace DeKayaServer.Application.Customers;

[Permission( "customer:create" )]
public sealed record CustomerCreateCommand(
    string FirstName,
    string LastName,
    Address Address,
    Contact Contact,
    bool IsActive
    ) : IRequest<Result<string>>;

public sealed class CustomerCreateCommandValidator : AbstractValidator<CustomerCreateCommand>
{
    public CustomerCreateCommandValidator()
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

internal sealed class CustomerCreateCommandHandler(
    ICustomerRepository customerRepository,
    IUnitOfWork unitOfWork ) : IRequestHandler<CustomerCreateCommand, Result<string>>
{
    public async Task<Result<string>> Handle( CustomerCreateCommand request, CancellationToken cancellationToken )
    {
        var phoneNumberExists = await customerRepository.AnyAsync( c => c.Contact.PhoneNumber == request.Contact.PhoneNumber, cancellationToken );
        if ( phoneNumberExists )
        {
            return Result<string>.Failure( "Bu telefon numarası zaten kayıtlı" );
        }

        FirstName firstName = new( request.FirstName );
        LastName lastName = new( request.LastName );
        Address address = request.Address;
        Contact contact = request.Contact;

        Customer customer = new( firstName, lastName, address, contact, request.IsActive );
        customerRepository.Add( customer );
        await unitOfWork.SaveChangesAsync( cancellationToken );
        return "Müşteri başarıyla oluşturuldu";
    }
}
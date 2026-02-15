using DeKayaServer.Domain.Abstractions;
using DeKayaServer.Domain.Users;
using DeKayaServer.Domain.Users.ValueObjects;
using FluentValidation;
using GenericRepository;
using TS.MediatR;
using TS.Result;

namespace DeKayaServer.Application.Users;

public sealed record class UserCreateCommand(
    string FirstName,
    string LastName,
    string Email,
    string UserName,
    Guid RoleId ) : IRequest<Result<string>>;

public sealed class UserCreateCommandValidator : AbstractValidator<UserCreateCommand>
{
    public UserCreateCommandValidator()
    {
        RuleFor( x => x.FirstName )
            .NotEmpty()
            .WithMessage( "Geçerli bir ad girin" );
        RuleFor( x => x.LastName )
            .NotEmpty()
            .WithMessage( "Geçerli bir soyad girin" );
        RuleFor( x => x.UserName )
            .NotEmpty()
            .WithMessage( "Geçerli bir kullanıcı adı girin" );
        RuleFor( x => x.Email )
            .NotEmpty().WithMessage( "Geçerli bir e-mail adresi girin" )
            .EmailAddress().WithMessage( "Geçerli bir e-mail adresi girin" );
    }
}

internal sealed class UserCreateCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork ) : IRequestHandler<UserCreateCommand, Result<string>>
{
    public async Task<Result<string>> Handle( UserCreateCommand request, CancellationToken cancellationToken )
    {
        var emailExists = await userRepository.AnyAsync( x => x.Email.Value == request.Email, cancellationToken );
        if ( emailExists )
        {
            return Result<string>.Failure( "Bu e-mail adresi zaten kullanılıyor" );
        }

        var userNameExists = await userRepository.AnyAsync( x => x.UserName.Value == request.UserName, cancellationToken );
        if ( emailExists )
        {
            return Result<string>.Failure( "Bu kullanıcı adı zaten kullanılıyor" );
        }

        FirstName firstName = new( request.FirstName );
        LastName lastName = new( request.LastName );
        Email email = new( request.Email );
        UserName userName = new( request.UserName );
        Password password = new( "Dekaya2026!" );
        IdentityId roleId = new( request.RoleId );

        User user = new( firstName, lastName, email, userName, password, roleId );
        userRepository.Add( user );
        await unitOfWork.SaveChangesAsync( cancellationToken );
        return "Kullanıcı başarıyla oluşturuldu";
    }
}

using DeKayaServer.Domain.Abstractions;
using DeKayaServer.Domain.Users;
using DeKayaServer.Domain.Users.ValueObjects;
using FluentValidation;
using GenericRepository;
using TS.MediatR;
using TS.Result;

namespace DeKayaServer.Application.Users;

public sealed record UserUpdateCommand(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string UserName,
    Guid RoleId ) : IRequest<Result<string>>;

public sealed class UserUpdateCommandValidator : AbstractValidator<UserUpdateCommand>
{
    public UserUpdateCommandValidator()
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

internal sealed class UserUpdateCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork ) : IRequestHandler<UserUpdateCommand, Result<string>>
{
    public async Task<Result<string>> Handle( UserUpdateCommand request, CancellationToken cancellationToken )
    {
        var user = await userRepository.FirstOrDefaultAsync( x => x.Id == request.Id, cancellationToken );
        if ( user is null )
        {
            return Result<string>.Failure( "Kullanıcı bulunamadı" );
        }

        if ( user.Email.Value != request.Email )
        {
            var emailExists = await userRepository.AnyAsync( x => x.Email.Value == request.Email, cancellationToken );
            if ( emailExists )
            {
                return Result<string>.Failure( "Bu e-mail adresi zaten kullanılıyor" );
            }
        }

        if ( user.UserName.Value != request.UserName )
        {
            var userNameExists = await userRepository.AnyAsync( x => x.UserName.Value == request.UserName, cancellationToken );
            if ( userNameExists )
            {
                return Result<string>.Failure( "Bu kullanıcı adı zaten kullanılıyor" );
            }
        }

        FirstName firstName = new( request.FirstName );
        LastName lastName = new( request.LastName );
        Email email = new( request.Email );
        UserName userName = new( request.UserName );
        IdentityId roleId = new( request.RoleId );

        user.SetFirstName( firstName );
        user.SetLastName( lastName );
        user.SetEmail( email );
        user.SetUserName( userName );
        user.SetRoleId( roleId );

        userRepository.Update( user );
        await unitOfWork.SaveChangesAsync( cancellationToken );
        return "Kullanıcı başarıyla güncellendi";
    }
}


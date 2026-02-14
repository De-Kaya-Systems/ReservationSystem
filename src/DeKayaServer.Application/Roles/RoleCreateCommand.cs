using DeKayaServer.Application.Behaviors;
using DeKayaServer.Domain.Role;
using DeKayaServer.Domain.Shared;
using FluentValidation;
using GenericRepository;
using TS.MediatR;
using TS.Result;

namespace DeKayaServer.Application.Roles;

[Permission( "role:create" )]
public sealed record RoleCreateCommand(
    string Name,
    bool IsActive ) : IRequest<Result<string>>;

//Validation kuralı
//EN: Validation rule
public sealed class RoleCreateCommandValidator : AbstractValidator<RoleCreateCommand>
{
    public RoleCreateCommandValidator()
    {
        RuleFor( x => x.Name )
            .NotEmpty()
            .WithMessage( "Role name cannot be empty." );
    }
}

//Handler
internal sealed class RoleCreateCommandHandler(
    IRoleRepository roleRepository,
    IUnitOfWork unitOfWork ) : IRequestHandler<RoleCreateCommand, Result<string>>
{
    public async Task<Result<string>> Handle( RoleCreateCommand request, CancellationToken cancellationToken )
    {
        var nameExists = await roleRepository.AnyAsync( r => r.Name.Value == request.Name, cancellationToken );

        if ( nameExists )
        {
            return Result<string>.Failure( "A role with the same name already exists." );
        }

        Name name = new( request.Name );
        Role role = new( name, request.IsActive );
        roleRepository.Add( role );
        await unitOfWork.SaveChangesAsync( cancellationToken );

        return Result<string>.Succeed( "Role successfully added" );
    }
}
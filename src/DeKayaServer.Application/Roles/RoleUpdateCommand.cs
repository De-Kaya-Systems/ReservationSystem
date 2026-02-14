using DeKayaServer.Application.Behaviors;
using DeKayaServer.Domain.Role;
using DeKayaServer.Domain.Shared;
using FluentValidation;
using GenericRepository;
using TS.MediatR;
using TS.Result;

namespace DeKayaServer.Application.Roles;

[Permission( "role:edit" )]
public sealed record RoleUpdateCommand(
    Guid Id,
    string Name,
    bool IsActive ) : IRequest<Result<string>>;

//Validation kuralı
//EN: Validation rule
public sealed class RoleUpdateCommandValidator : AbstractValidator<RoleUpdateCommand>
{
    public RoleUpdateCommandValidator()
    {
        RuleFor( x => x.Name )
            .NotEmpty()
            .WithMessage( "Role name cannot be empty." );
    }
}

//Handler
internal sealed class RoleUpdateCommandHandler(
    IRoleRepository roleRepository,
    IUnitOfWork unitOfWork ) : IRequestHandler<RoleUpdateCommand, Result<string>>
{
    public async Task<Result<string>> Handle( RoleUpdateCommand request, CancellationToken cancellationToken )
    {
        var role = await roleRepository.FirstOrDefaultAsync( r => r.Id == request.Id, cancellationToken );
        if ( role is null )
        {
            return Result<string>.Failure( "Role not found." );
        }

        Name name = new( request.Name );
        role.SetName( name );
        role.SetStatus( request.IsActive );

        roleRepository.Update( role );
        await unitOfWork.SaveChangesAsync( cancellationToken );
        return Result<string>.Succeed( "Role successfully updated" );
    }
}
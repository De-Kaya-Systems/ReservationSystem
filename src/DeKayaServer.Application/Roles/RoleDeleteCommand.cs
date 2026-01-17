using DeKayaServer.Application.Behaviors;
using DeKayaServer.Domain.Role;
using GenericRepository;
using TS.MediatR;
using TS.Result;

namespace DeKayaServer.Application.Roles;

[Permission("role:delete")]
public sealed record RoleDeleteCommand(Guid Id) : IRequest<Result<string>>;

internal sealed class RoleDeleteCommandHandler(
    IRoleRepository roleRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<RoleDeleteCommand, Result<string>>
{
    public async Task<Result<string>> Handle(RoleDeleteCommand request, CancellationToken cancellationToken)
    {
        var role = await roleRepository.FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);
        if (role is null)
        {
            return Result<string>.Failure("Role not found.");
        }
        role.Delete();
        roleRepository.Update(role);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<string>.Succeed("Role successfully deleted");
    }
}
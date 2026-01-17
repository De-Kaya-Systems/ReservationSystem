using DeKayaServer.Domain.Role;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace DeKayaServer.Application.Roles;

public sealed record RoleGetQuery(Guid Id) : IRequest<Result<RoleDto>>;

internal sealed class RoleGetQueryHandler(
    IRoleRepository roleRepository) : IRequestHandler<RoleGetQuery, Result<RoleDto>>
{
    public async Task<Result<RoleDto>> Handle(RoleGetQuery request, CancellationToken cancellationToken)
    {
        var res = await roleRepository
            .GetAllWithAudit()
            .MapTo()
            .Where(r => r.Id == request.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (res is null)
        {
            return Result<RoleDto>.Failure("Role not found");
        }

        return res;
    }
}
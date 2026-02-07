using DeKayaServer.Application.Behaviors;
using DeKayaServer.Contracts.Roles;
using DeKayaServer.Domain.Role;
using TS.MediatR;

namespace DeKayaServer.Application.Roles;

[Permission( "role:view" )]
public sealed record RoleGetAllQuery : IRequest<IQueryable<RoleDto>>;

internal sealed class RoleGetAllQueryHandler(
    IRoleRepository roleRepository ) : IRequestHandler<RoleGetAllQuery, IQueryable<RoleDto>>
{
    public Task<IQueryable<RoleDto>> Handle( RoleGetAllQuery request, CancellationToken cancellationToken )
    {
        var query = roleRepository
            .GetAllWithAudit()
            .MapToDto()
            .AsQueryable();

        return Task.FromResult( query );
    }
}

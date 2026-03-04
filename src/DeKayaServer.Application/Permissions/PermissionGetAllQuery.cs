using DeKayaServer.Application.Behaviors;
using DeKayaServer.Application.Services;
using TS.MediatR;
using TS.Result;

namespace DeKayaServer.Application.Permissions;

[Permission( "permission:getall" )]
public sealed record PermissionGetAllQuery : IRequest<Result<List<string>>>;

internal sealed class PermissionGetAllQueryHandler(
    PermissionService permissionService ) : IRequestHandler<PermissionGetAllQuery, Result<List<string>>>
{
    public Task<Result<List<string>>> Handle( PermissionGetAllQuery request, CancellationToken cancellationToken )
        => Task.FromResult( Result<List<string>>.Succeed( permissionService.GetAll() ) );
}

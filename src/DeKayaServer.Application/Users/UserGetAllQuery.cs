using DeKayaServer.Domain.Role;
using DeKayaServer.Domain.Users;
using TS.MediatR;

namespace DeKayaServer.Application.Users;

public sealed record UserGetAllQuery : IRequest<IQueryable<UserDto>>;

internal sealed class UserGetAllQueryHandler(
    IUserRepository userRepository,
    IRoleRepository roleRepository ) : IRequestHandler<UserGetAllQuery, IQueryable<UserDto>>
{
    public Task<IQueryable<UserDto>> Handle( UserGetAllQuery request, CancellationToken cancellationToken )
    {
        var res = userRepository.GetAllWithAudit().MapTo( roleRepository.GetAll() );
        return Task.FromResult( res );
    }
}
using DeKayaServer.Domain.Role;
using DeKayaServer.Domain.Users;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace DeKayaServer.Application.Users;

public sealed record UserGetQuery(
    Guid Id ) : IRequest<Result<UserDto>>;

internal sealed class UserGetQueryHandler(
    IUserRepository userRepository,
    IRoleRepository roleRepository ) : IRequestHandler<UserGetQuery, Result<UserDto>>
{
    public async Task<Result<UserDto>> Handle( UserGetQuery request, CancellationToken cancellationToken )
    {
        var res = await userRepository.GetAllWithAudit().MapTo( roleRepository.GetAll() )
        .Where( x => x.Id == request.Id )
        .FirstOrDefaultAsync( cancellationToken );

        if ( res is null )
        {
            return Result<UserDto>.Failure( "Kullanıcı bulunamadı" );
        }
        return res;
    }
}

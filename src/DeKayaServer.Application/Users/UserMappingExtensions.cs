using DeKayaServer.Contracts.Users;
using DeKayaServer.Domain.Abstractions;
using DeKayaServer.Domain.Role;
using DeKayaServer.Domain.Users;

namespace DeKayaServer.Application.Users;

public static class UserMappingExtensions
{
    public static IQueryable<UserDto> MapTo(
        this IQueryable<EntityWithAuditDto<User>> entities,
        IQueryable<Role> roles )
    {
        var res = entities
            .Join( roles, y => y.Entity.RoleId, y => y.Id, ( e, role )
                => new { e.Entity, e.CreatedUser, e.UpdatedUser, Role = role } )
            .Select( x => new UserDto
            {
                Id = x.Entity.Id,
                FirstName = x.Entity.FirstName.Value,
                LastName = x.Entity.LastName.Value,
                FullName = x.Entity.FullName.Value,
                Email = x.Entity.Email.Value,
                UserName = x.Entity.UserName.Value,
                RoleName = x.Role.Name.Value,
                RoleId = x.Role.Id,
                CreatedAt = x.Entity.CreatedAt,
                CreatedBy = x.Entity.CreatedBy.Value,
                CreatedFullName = x.CreatedUser.FullName.Value,
                UpdatedAt = x.Entity.UpdatedAt,
                UpdatedBy = x.Entity.UpdatedBy != null ? x.Entity.UpdatedBy.Value : null,
                UpdatedFullName = x.UpdatedUser != null ? x.UpdatedUser.FullName.Value : null,
            } );
        return res;
    }
}
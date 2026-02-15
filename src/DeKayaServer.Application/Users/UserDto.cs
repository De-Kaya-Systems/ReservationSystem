using DeKayaServer.Domain.Abstractions;
using DeKayaServer.Domain.Role;
using DeKayaServer.Domain.Users;

namespace DeKayaServer.Application.Users;

public sealed class UserDto : EntityDto
{
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string UserName { get; set; } = default!;
    public string RoleName { get; set; } = default!;
}

public static class UserExtensions
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
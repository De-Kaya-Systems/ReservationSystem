using DeKayaServer.Domain.Abstractions;
using DeKayaServer.Domain.Role;

namespace DeKayaServer.Application.Roles;

public sealed class RoleDto : EntityDto
{
    public string Name { get; set; } = default!;
}

//Bu method, Role entity'lerini RoleDto'larına dönüştürmek için kullanılır.
//EN: This method is used to map Role entities to RoleDto.
public static class RoleExtensions
{
    public static IQueryable<RoleDto> MapTo(this IQueryable<EntityWithAuditDto<Role>> entites)
    {
        var res = entites.Select(s => new RoleDto
        {
            Id = s.Entity.Id,
            Name = s.Entity.Name.Value,
            IsActive = s.Entity.IsActive,
            CreatedAt = s.Entity.CreatedAt,
            CreatedBy = s.Entity.CreatedBy,
            CreatedFullName = s.CreatedUser.FullName.Value,
            UpdatedAt = s.Entity.UpdatedAt,
            UpdatedBy = s.Entity.UpdatedBy != null ? s.Entity.UpdatedBy.Value : null,
            UpdatedFullName = s.UpdatedUser != null ? s.UpdatedUser.FullName.Value : null,
        }).AsQueryable();

        return res;
    }
}

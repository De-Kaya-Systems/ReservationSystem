using DeKayaServer.Contracts.Roles;
using DeKayaServer.Domain.Abstractions;
using DeKayaServer.Domain.Role;

namespace DeKayaServer.Application.Roles;

public static class RoleMappingExtensions
{
    /// <summary>
    /// Maps Role entities (with audit user joins) to shared RoleDto.
    /// TR: Role varlıklarını (denetim kullanıcı birleşimleriyle) paylaşılan RoleDto'ya dönüştürür.
    /// </summary>
    public static IQueryable<RoleDto> MapToDto( this IQueryable<EntityWithAuditDto<Role>> entities )
    {
        return entities.Select( s => new RoleDto
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
        } );
    }
}

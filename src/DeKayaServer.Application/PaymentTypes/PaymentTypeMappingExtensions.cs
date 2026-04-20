using DeKayaServer.Contracts.PaymentTypes;
using DeKayaServer.Domain.Abstractions;

namespace DeKayaServer.Application.PaymentTypes;

public static class PaymentTypeMappingExtensions
{
    public static IQueryable<PaymentTypeDto> MapToDto(
        this IQueryable<EntityWithAuditDto<DeKayaServer.Domain.PaymentTypes.PaymentTypes>> entities )
    {
        return entities.Select( s => new PaymentTypeDto
        {
            Id = s.Entity.Id,
            Name = s.Entity.PaymentType.Value,
            IsActive = s.Entity.IsActive,
            CreatedAt = s.Entity.CreatedAt,
            CreatedBy = s.Entity.CreatedBy,
            CreatedFullName = s.CreatedUser.FullName.Value,
            UpdatedAt = s.Entity.UpdatedAt,
            UpdatedBy = s.Entity.UpdatedBy != null ? s.Entity.UpdatedBy.Value : null,
            UpdatedFullName = s.UpdatedUser != null ? s.UpdatedUser.FullName.Value : null
        } );
    }
}
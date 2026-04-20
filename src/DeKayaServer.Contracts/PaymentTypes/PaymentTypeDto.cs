using DeKayaServer.Contracts.Dto;

namespace DeKayaServer.Contracts.PaymentTypes;

public sealed class PaymentTypeDto : EntityDto
{
    public string Name { get; set; } = default!;
}
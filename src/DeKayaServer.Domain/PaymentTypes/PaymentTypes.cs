using DeKayaServer.Domain.Abstractions;

namespace DeKayaServer.Domain.PaymentTypes;

public class PaymentTypes : Entity
{
    public PaymentTypes() { }

    public PaymentTypes(
        PaymentType paymentType,
        bool isActive )
    {
        SetPaymentType( paymentType );
        SetStatus( isActive );
    }

    public PaymentType PaymentType { get; private set; } = default!;

    #region Behaviors
    public void SetPaymentType( PaymentType paymentType )
    {
        PaymentType = paymentType;
    }
    #endregion
}
public sealed record PaymentType( string Value );
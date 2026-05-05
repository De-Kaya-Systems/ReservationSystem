using DeKayaServer.Domain.Abstractions;
using DeKayaServer.Domain.CustomerBalance.Enum;
using DeKayaServer.Domain.CustomerBalance.ValueObjects;

namespace DeKayaServer.Domain.PaymentHistory;

public sealed class PaymentHistory : Entity
{
    private PaymentHistory() { }

    private PaymentHistory(
        IdentityId customerId,
        BalanceSourceType sourceType,
        IdentityId? sourceId,
        IdentityId paymentTypeId,
        decimal paymentAmount,
        decimal remainingBalance,
        DateTime paymentDate,
        string? notes )
    {
        SetCustomerId( customerId );
        SetSourceType( sourceType );
        SetSourceId( sourceId );
        SetPaymentTypeId( paymentTypeId );
        SetPaymentAmount( paymentAmount );
        SetRemainingBalance( remainingBalance );
        SetPaymentDate( paymentDate );
        SetNotes( notes );
    }

    public static PaymentHistory Create(
        IdentityId customerId,
        BalanceSourceType sourceType,
        IdentityId? sourceId,
        IdentityId paymentTypeId,
        decimal paymentAmount,
        decimal remainingBalance,
        DateTime paymentDate,
        string? notes = null )
    {
        ValidatePaymentAmounts( paymentAmount, remainingBalance );
        
        var history = new PaymentHistory(
            customerId,
            sourceType,
            sourceId,
            paymentTypeId,
            paymentAmount,
            remainingBalance,
            paymentDate,
            notes );
        
        return history;
    }

    public IdentityId CustomerId { get; private set; } = default!;
    public BalanceSourceType SourceType { get; private set; }
    public IdentityId? SourceId { get; private set; }
    public IdentityId PaymentTypeId { get; private set; } = default!;
    public decimal PaymentAmount { get; private set; }
    public decimal RemainingBalance { get; private set; }
    public DateTime PaymentDate { get; private set; }
    public string? Notes { get; private set; }

    #region Behaviors

    public void SetCustomerId( IdentityId customerId )
    {
        CustomerId = customerId;
    }

    public void SetSourceType( BalanceSourceType sourceType )
    {
        SourceType = sourceType;
    }

    public void SetSourceId( IdentityId? sourceId )
    {
        SourceId = sourceId;
    }

    public void SetPaymentTypeId( IdentityId paymentTypeId )
    {
        PaymentTypeId = paymentTypeId;
    }

    public void SetPaymentAmount( decimal paymentAmount )
    {
        if ( paymentAmount < 0 )
        {
            throw new ArgumentException( "Ödeme tutarı negatif olamaz." );
        }

        PaymentAmount = paymentAmount;
    }

    public void SetRemainingBalance( decimal remainingBalance )
    {
        if ( remainingBalance < 0 )
        {
            throw new ArgumentException( "Kalan borç negatif olamaz." );
        }

        RemainingBalance = remainingBalance;
    }

    public void SetPaymentDate( DateTime paymentDate )
    {
        PaymentDate = paymentDate;
    }

    public void SetNotes( string? notes )
    {
        Notes = notes;
    }

    private static void ValidatePaymentAmounts( decimal paymentAmount, decimal remainingBalance )
    {
        if ( paymentAmount < 0 )
        {
            throw new ArgumentException( "Ödeme tutarı negatif olamaz." );
        }

        if ( remainingBalance < 0 )
        {
            throw new ArgumentException( "Kalan borç negatif olamaz." );
        }
    }

    #endregion
}

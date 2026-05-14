using DeKayaServer.Domain.Abstractions;
using DeKayaServer.Domain.CustomerAccountAdjustments.Enum;
using DeKayaServer.Domain.CustomerAccountAdjustments.ValueObjects;
using DeKayaServer.Domain.CustomerBalance.Enum;

namespace DeKayaServer.Domain.CustomerAccountAdjustments;

public sealed class CustomerAccountAdjustment : Entity
{
    private CustomerAccountAdjustment() { }

    private CustomerAccountAdjustment(
        IdentityId customerId,
        IdentityId customerBalanceId,
        BalanceSourceType sourceType,
        IdentityId? sourceId,
        CustomerAccountAdjustmentType adjustmentType,
        CustomerAccountAdjustmentDirection direction,
        AdjustmentAmount amount,
        AdjustmentReason reason,
        AdjustmentNote? note,
        decimal balanceBeforeAdjustment,
        decimal balanceAfterAdjustment )
    {
        SetCustomerId( customerId );
        SetCustomerBalanceId( customerBalanceId );
        SetSourceType( sourceType );
        SetSourceId( sourceId );
        SetAdjustmentType( adjustmentType );
        SetDirection( direction );
        SetAmount( amount );
        SetReason( reason );
        SetNote( note );
        SetBalanceBeforeAdjustment( balanceBeforeAdjustment );
        SetBalanceAfterAdjustment( balanceAfterAdjustment );
    }

    public static CustomerAccountAdjustment Create(
       IdentityId customerId,
       IdentityId customerBalanceId,
       BalanceSourceType sourceType,
       IdentityId? sourceId,
       CustomerAccountAdjustmentType adjustmentType,
       CustomerAccountAdjustmentDirection direction,
       AdjustmentAmount amount,
       AdjustmentReason reason,
       AdjustmentNote? note,
       decimal balanceBeforeAdjustment,
       decimal balanceAfterAdjustment )
    {
        Validate(
            amount,
            reason,
            balanceBeforeAdjustment,
            balanceAfterAdjustment );

        return new CustomerAccountAdjustment(
            customerId,
            customerBalanceId,
            sourceType,
            sourceId,
            adjustmentType,
            direction,
            amount,
            reason,
            note,
            balanceBeforeAdjustment,
            balanceAfterAdjustment );
    }

    public IdentityId CustomerId { get; private set; } = default!;
    public IdentityId CustomerBalanceId { get; private set; } = default!;

    public BalanceSourceType SourceType { get; private set; }
    public IdentityId? SourceId { get; private set; }

    public CustomerAccountAdjustmentType AdjustmentType { get; private set; }
    public CustomerAccountAdjustmentDirection Direction { get; private set; }
    public AdjustmentAmount Amount { get; private set; } = default!;
    public AdjustmentReason Reason { get; private set; } = default!;
    public AdjustmentNote? Note { get; private set; }

    public decimal BalanceBeforeAdjustment { get; private set; }
    public decimal BalanceAfterAdjustment { get; private set; }

    private void SetCustomerId( IdentityId customerId )
    {
        CustomerId = customerId;
    }

    private void SetCustomerBalanceId( IdentityId customerBalanceId )
    {
        CustomerBalanceId = customerBalanceId;
    }

    private void SetSourceType( BalanceSourceType sourceType )
    {
        SourceType = sourceType;
    }

    private void SetSourceId( IdentityId? sourceId )
    {
        SourceId = sourceId;
    }

    private void SetAdjustmentType( CustomerAccountAdjustmentType adjustmentType )
    {
        AdjustmentType = adjustmentType;
    }

    private void SetDirection( CustomerAccountAdjustmentDirection direction )
    {
        Direction = direction;
    }

    private void SetAmount( AdjustmentAmount amount )
    {
        Amount = amount;
    }

    private void SetReason( AdjustmentReason reason )
    {
        Reason = reason;
    }

    private void SetNote( AdjustmentNote? note )
    {
        Note = note;
    }

    private void SetBalanceBeforeAdjustment( decimal balanceBeforeAdjustment )
    {
        BalanceBeforeAdjustment = balanceBeforeAdjustment;
    }

    private void SetBalanceAfterAdjustment( decimal balanceAfterAdjustment )
    {
        BalanceAfterAdjustment = balanceAfterAdjustment;
    }

    private static void Validate(
        AdjustmentAmount amount,
        AdjustmentReason reason,
        decimal balanceBeforeAdjustment,
        decimal balanceAfterAdjustment )
    {
        if ( amount.Value <= 0 )
        {
            throw new ArgumentException( "Düzeltme tutarı sıfırdan büyük olmalıdır." );
        }

        if ( string.IsNullOrWhiteSpace( reason.Value ) )
        {
            throw new ArgumentException( "Düzeltme sebebi boş olamaz." );
        }

        if ( balanceBeforeAdjustment < 0 )
        {
            throw new ArgumentException( "İşlem öncesi bakiye negatif olamaz." );
        }

        if ( balanceAfterAdjustment < 0 )
        {
            throw new ArgumentException( "İşlem sonrası bakiye negatif olamaz." );
        }
    }
}

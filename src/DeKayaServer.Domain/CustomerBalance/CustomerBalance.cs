using DeKayaServer.Domain.Abstractions;
using DeKayaServer.Domain.CustomerBalance.ValueObjects;

namespace DeKayaServer.Domain.CustomerBalance;

public sealed class CustomerBalance : Entity
{
    public CustomerBalance() { }

    public CustomerBalance(
        IdentityId customerId,
        TotalAmount totalAmount,
        OutstandingAmount? outstandingAmount,
        PaidAmount? paidAmount,
        Description description,
        LastPaymentType? paymentType,
        LastPaymentAt? lastPaymentAt )
    {
        SetCustomerId( customerId );
        SetDescription( description );
        SetLastPaymentType( paymentType );
        SetLastPaymentAt( lastPaymentAt );

        SetAmounts( totalAmount, paidAmount, outstandingAmount );
    }

    public IdentityId CustomerId { get; private set; } = default!;
    public TotalAmount TotalAmount { get; private set; } = default!;
    public OutstandingAmount OutstandingAmount { get; private set; } = new( 0 );
    public PaidAmount PaidAmount { get; private set; } = new( 0 );
    public Description? Description { get; private set; }
    public BalanceStatus BalanceStatus { get; private set; } = BalanceStatus.Pending;
    public LastPaymentType? PaymentType { get; private set; }
    public LastPaymentAt? LastPaymentAt { get; private set; }

    #region Behaviors
    public void SetCustomerId( IdentityId customerId )
    {
        CustomerId = customerId;
    }

    public void SetTotalAmount( TotalAmount totalAmount )
    {
        TotalAmount = totalAmount;
        EnsureInvariantsAndSyncStatus();
    }

    public void SetOutstandingAmount( OutstandingAmount outstandingAmount )
    {
        OutstandingAmount = outstandingAmount;
        EnsureInvariantsAndSyncStatus();
    }

    public void SetPaidAmount( PaidAmount paidAmount )
    {
        PaidAmount = paidAmount;
        EnsureInvariantsAndSyncStatus();
    }

    public void SetDescription( Description? description )
    {
        Description = description;
    }

    public void SetLastPaymentType( LastPaymentType? paymentType )
    {
        PaymentType = paymentType;
    }

    public void SetLastPaymentAt( LastPaymentAt? lastPaymentAt )
    {
        LastPaymentAt = lastPaymentAt;
    }
    #endregion

    private void SetAmounts(
        TotalAmount totalAmount,
        PaidAmount? paidAmount,
        OutstandingAmount? outstandingAmount )
    {
        TotalAmount = totalAmount;

        if ( paidAmount is null && outstandingAmount is null )
        {
            PaidAmount = new( 0 );
            OutstandingAmount = new( totalAmount.Value );
        }
        else if ( paidAmount is null )
        {
            OutstandingAmount = outstandingAmount!;
            PaidAmount = new( totalAmount.Value - OutstandingAmount.Value );
        }
        else if ( outstandingAmount is null )
        {
            PaidAmount = paidAmount;
            OutstandingAmount = new( totalAmount.Value - PaidAmount.Value );
        }
        else
        {
            PaidAmount = paidAmount;
            OutstandingAmount = outstandingAmount;
        }

        EnsureInvariantsAndSyncStatus();
    }

    private void EnsureInvariantsAndSyncStatus()
    {
        if ( TotalAmount.Value < 0 || PaidAmount.Value < 0 || OutstandingAmount.Value < 0 )
        {
            throw new ArgumentException( "Tutar alanları negatif olamaz." );
        }

        if ( PaidAmount.Value + OutstandingAmount.Value != TotalAmount.Value )
        {
            throw new ArgumentException( "Kural ihlali: PaidAmount + OutstandingAmount, TotalAmount'a eşit olmalı." );
        }

        if ( OutstandingAmount.Value == 0 )
        {
            BalanceStatus = BalanceStatus.Paid;
            return;
        }

        if ( PaidAmount.Value == 0 )
        {
            BalanceStatus = BalanceStatus.Pending;
            return;
        }

        BalanceStatus = BalanceStatus.PartiallyPaid;
    }
}

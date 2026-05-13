using DeKayaServer.Application.Behaviors;
using DeKayaServer.Contracts.CustomerAccount;
using DeKayaServer.Domain.CustomerBalance;
using DeKayaServer.Domain.CustomerBalance.Enum;
using DeKayaServer.Domain.Customers;
using DeKayaServer.Domain.PaymentHistory;
using DeKayaServer.Domain.PaymentTypes;
using DeKayaServer.Domain.Reservations;
using Microsoft.EntityFrameworkCore;
using TS.MediatR;
using TS.Result;

namespace DeKayaServer.Application.Customers;

[Permission( "customer-account:get" )]
public sealed record CustomerAccountGetQuery(
    Guid CustomerId,
    DateTime? StartDate,
    DateTime? EndDate )
    : IRequest<Result<CustomerAccountDto>>;

internal sealed class CustomerAccountGetQueryHandler(
    ICustomerRepository customerRepository,
    ICustomerBalanceRepository customerBalanceRepository,
    IPaymentHistoryRepository paymentHistoryRepository,
    IPaymentTypesRepository paymentTypesRepository,
    IReservationRepository reservationRepository )
    : IRequestHandler<CustomerAccountGetQuery, Result<CustomerAccountDto>>
{
    public async Task<Result<CustomerAccountDto>> Handle(
        CustomerAccountGetQuery request,
        CancellationToken cancellationToken )
    {
        var periodStart = request.StartDate?.Date;
        var periodEndExclusive = request.EndDate?.Date.AddDays( 1 );

        if ( periodStart.HasValue
             && periodEndExclusive.HasValue
             && periodStart.Value >= periodEndExclusive.Value )
        {
            return Result<CustomerAccountDto>.Failure(
                "Başlangıç tarihi bitiş tarihinden sonra olamaz." );
        }

        var customer = await customerRepository
            .GetAll()
            .Where( x => x.Id == request.CustomerId )
            .Select( x => new
            {
                CustomerId = x.Id.Value,
                FullName = x.FullName.Value,
                x.Contact.PhoneNumber,
                x.Contact.Email
            } )
            .FirstOrDefaultAsync( cancellationToken );

        if ( customer is null )
        {
            return Result<CustomerAccountDto>.Failure( "Müşteri bulunamadı." );
        }

        var paymentTypeNames = await paymentTypesRepository
            .GetAll()
            .Select( x => new
            {
                Id = x.Id.Value,
                Name = x.PaymentType.Value
            } )
            .ToDictionaryAsync( x => x.Id, x => x.Name, cancellationToken );

        var balances = await customerBalanceRepository
            .GetAll()
            .Where( x => x.CustomerId == request.CustomerId )
            .OrderByDescending( x => x.CreatedAt )
            .Select( x => new CustomerAccountBalanceItemDto
            {
                Id = x.Id.Value,
                SourceType = x.SourceType.ToString(),
                SourceId = x.SourceId != null ? x.SourceId.Value : null,
                PaymentTypeId = x.PaymentTypeId.Value,
                TotalAmount = x.TotalAmount.Value,
                PaidAmount = x.PaidAmount.Value,
                OutstandingAmount = x.OutstandingAmount.Value,
                BalanceStatus = x.BalanceStatus.Value,
                Description = x.Description != null ? x.Description.Value : null,
                LastPaymentAt = x.LastPaymentAt != null ? x.LastPaymentAt.Value : null,
                CreatedAt = x.CreatedAt
            } )
            .ToListAsync( cancellationToken );

        var payments = await paymentHistoryRepository
            .GetAll()
            .Where( x => x.CustomerId == request.CustomerId )
            .OrderByDescending( x => x.PaymentDate )
            .Select( x => new CustomerAccountPaymentHistoryItemDto
            {
                Id = x.Id.Value,
                SourceType = x.SourceType.ToString(),
                SourceId = x.SourceId != null ? x.SourceId.Value : null,
                PaymentTypeId = x.PaymentTypeId.Value,
                PaymentAmount = x.PaymentAmount,
                RemainingBalance = x.RemainingBalance,
                PaymentDate = x.PaymentDate,
                Notes = x.Notes
            } )
            .ToListAsync( cancellationToken );

        foreach ( var balance in balances )
        {
            balance.PaymentTypeName = paymentTypeNames.GetValueOrDefault( balance.PaymentTypeId );
        }

        foreach ( var payment in payments )
        {
            payment.PaymentTypeName = paymentTypeNames.GetValueOrDefault( payment.PaymentTypeId );
        }

        var reservationSourcesIds = balances
            .Where( x => x.SourceType == BalanceSourceType.Reservation.ToString() && x.SourceId.HasValue )
            .Select( x => x.SourceId!.Value )
            .Concat( payments
                .Where( x => x.SourceType == BalanceSourceType.Reservation.ToString() && x.SourceId.HasValue )
                .Select( x => x.SourceId!.Value ) )
            .Distinct()
            .ToList();

        var customerReservations = await reservationRepository
            .GetAll()
            .Where( x => x.CustomerId == request.CustomerId )
            .Select( x => new
            {
                Id = x.Id.Value,
                ReservationNumber = x.ReservationNumber.Value
            } )
            .ToListAsync( cancellationToken );

        var reservationNumbers = customerReservations
            .Where( x => reservationSourcesIds.Contains( x.Id ) )
            .ToDictionary( x => x.Id, x => x.ReservationNumber );

        var statementEvents = balances
            .Select( x => new AccountStatementEvent(
                TransactionDate: x.CreatedAt.DateTime,
                SortOrder: 0,
                TransactionType: "Borç",
                SourceType: x.SourceType,
                SourceId: x.SourceId,
                ReservationNumber: GetReservationNumber( reservationNumbers, x.SourceType, x.SourceId ),
                PaymentTypeId: x.PaymentTypeId,
                PaymentTypeName: x.PaymentTypeName,
                Description: x.Description,
                DebitAmount: x.TotalAmount,
                CreditAmount: 0 ) )
            .Concat( payments.Select( x => new AccountStatementEvent(
                TransactionDate: x.PaymentDate,
                SortOrder: 1,
                TransactionType: "Ödeme",
                SourceType: x.SourceType,
                SourceId: x.SourceId,
                ReservationNumber: GetReservationNumber( reservationNumbers, x.SourceType, x.SourceId ),
                PaymentTypeId: x.PaymentTypeId,
                PaymentTypeName: x.PaymentTypeName,
                Description: x.Notes,
                DebitAmount: 0,
                CreditAmount: x.PaymentAmount ) ) )
            .OrderBy( x => x.TransactionDate )
            .ThenBy( x => x.SortOrder )
            .ToList();

        var openingBalance = statementEvents
            .Where( x => periodStart.HasValue && x.TransactionDate < periodStart.Value )
            .Sum( x => x.DebitAmount - x.CreditAmount );

        var runningBalance = openingBalance;

        var statementItems = statementEvents
            .Where( x => IsInPeriod( x.TransactionDate, periodStart, periodEndExclusive ) )
            .Select( x =>
            {
                runningBalance += x.DebitAmount - x.CreditAmount;

                return new CustomerAccountStatementItemDto
                {
                    TransactionDate = x.TransactionDate,
                    TransactionType = x.TransactionType,
                    SourceType = x.SourceType,
                    SourceId = x.SourceId,
                    ReservationNumber = x.ReservationNumber,
                    PaymentTypeId = x.PaymentTypeId,
                    PaymentTypeName = x.PaymentTypeName,
                    Description = x.Description,
                    DebitAmount = x.DebitAmount,
                    CreditAmount = x.CreditAmount,
                    BalanceAfterTransaction = runningBalance
                };
            } )
            .ToList();

        return new CustomerAccountDto
        {
            CustomerId = customer.CustomerId,
            CustomerFullName = customer.FullName,
            PhoneNumber = customer.PhoneNumber,
            Email = customer.Email,

            PeriodStartDate = periodStart,
            PeriodEndDate = request.EndDate?.Date,

            CurrentOutstandingAmount = balances.Sum( x => x.OutstandingAmount ),
            TotalPaidAmount = payments.Sum( x => x.PaymentAmount ),
            LastPaymentDate = payments.FirstOrDefault()?.PaymentDate,

            OpeningBalance = openingBalance,
            PeriodDebtAmount = statementItems.Sum( x => x.DebitAmount ),
            PeriodPaidAmount = statementItems.Sum( x => x.CreditAmount ),
            PeriodNetAmount = statementItems.Sum( x => x.DebitAmount - x.CreditAmount ),
            ClosingBalance = runningBalance,

            StatementItems = statementItems,
            BalanceItems = balances,
            PaymentHistories = payments
        };
    }

    private static string? GetReservationNumber(
        IReadOnlyDictionary<Guid, string> reservationNumbers,
        string sourceType,
        Guid? sourceId )
    {
        if ( !sourceId.HasValue )
        {
            return null;
        }

        if ( sourceType != BalanceSourceType.Reservation.ToString() )
        {
            return null;
        }

        return reservationNumbers.GetValueOrDefault( sourceId.Value );
    }

    private static bool IsInPeriod(
        DateTime value,
        DateTime? periodStart,
        DateTime? periodEndExclusive )
    {
        if ( periodStart.HasValue && value < periodStart.Value )
        {
            return false;
        }

        if ( periodEndExclusive.HasValue && value >= periodEndExclusive.Value )
        {
            return false;
        }

        return true;
    }

    private sealed record AccountStatementEvent(
        DateTime TransactionDate,
        int SortOrder,
        string TransactionType,
        string SourceType,
        Guid? SourceId,
        string? ReservationNumber,
        Guid? PaymentTypeId,
        string? PaymentTypeName,
        string? Description,
        decimal DebitAmount,
        decimal CreditAmount );
}

using DeKayaServer.Application.Behaviors;
using DeKayaServer.Contracts.CustomerAccount;
using DeKayaServer.Domain.Abstractions;
using DeKayaServer.Domain.CustomerBalance;
using DeKayaServer.Domain.CustomerBalance.ValueObjects;
using DeKayaServer.Domain.Customers;
using DeKayaServer.Domain.PaymentHistory;
using DeKayaServer.Domain.PaymentTypes;
using FluentValidation;
using GenericRepository;
using TS.MediatR;
using TS.Result;

namespace DeKayaServer.Application.Customers;

[Permission( "customer-account:payment-receive" )]
public sealed record ReceiveCustomerPaymentCommand(
    Guid CustomerId,
    Guid CustomerBalanceId,
    Guid PaymentTypeId,
    decimal PaymentAmount,
    DateTime PaymentDate,
    string? Notes ) : IRequest<Result<string>>
{
    public static ReceiveCustomerPaymentCommand FromRequest(
            Guid customerId,
            ReceiveCustomerPaymentRequest request )
            => new(
                CustomerId: customerId,
                CustomerBalanceId: request.CustomerBalanceId,
                PaymentTypeId: request.PaymentTypeId,
                PaymentAmount: request.PaymentAmount,
                PaymentDate: request.PaymentDate,
                Notes: request.Notes );
}

public sealed class ReceiveCustomerPaymentCommandValidator
    : AbstractValidator<ReceiveCustomerPaymentCommand>
{
    public ReceiveCustomerPaymentCommandValidator()
    {
        RuleFor( x => x.CustomerId )
            .NotEmpty()
            .WithMessage( "Geçerli bir müşteri seçin." );

        RuleFor( x => x.CustomerBalanceId )
            .NotEmpty()
            .WithMessage( "Ödeme yapılacak borç kalemi seçilmelidir." );

        RuleFor( x => x.PaymentTypeId )
            .NotEmpty()
            .WithMessage( "Ödeme tipi seçilmelidir." );

        RuleFor( x => x.PaymentAmount )
            .GreaterThan( 0 )
            .WithMessage( "Ödeme tutarı 0'dan büyük olmalıdır." );

        RuleFor( x => x.PaymentDate )
            .NotEmpty()
            .WithMessage( "Ödeme tarihi zorunludur." );

        RuleFor( x => x.Notes )
            .MaximumLength( 500 )
            .WithMessage( "Açıklama en fazla 500 karakter olabilir." );
    }
}

internal sealed class ReceiveCustomerPaymentCommandHandler(
    ICustomerRepository customerRepository,
    ICustomerBalanceRepository customerBalanceRepository,
    IPaymentTypesRepository paymentTypesRepository,
    IPaymentHistoryRepository paymentHistoryRepository,
    IUnitOfWork unitOfWork )
    : IRequestHandler<ReceiveCustomerPaymentCommand, Result<string>>
{
    public async Task<Result<string>> Handle(
        ReceiveCustomerPaymentCommand request,
        CancellationToken cancellationToken )
    {
        var customerExists = await customerRepository.AnyAsync(
            x => x.Id == request.CustomerId,
            cancellationToken );

        if ( !customerExists )
        {
            return Result<string>.Failure( "Müşteri bulunamadı." );
        }

        var customerBalance = await customerBalanceRepository.FirstOrDefaultAsync(
            x => x.Id == request.CustomerBalanceId,
            cancellationToken );

        if ( customerBalance is null )
        {
            return Result<string>.Failure( "Borç kalemi bulunamadı." );
        }

        if ( customerBalance.CustomerId.Value != request.CustomerId )
        {
            return Result<string>.Failure( "Borç kalemi müşteriye ait değil." );
        }

        if ( customerBalance.OutstandingAmount.Value <= 0 )
        {
            return Result<string>.Failure( "Bu borç kalemi zaten kapatılmış." );
        }

        if ( request.PaymentAmount > customerBalance.OutstandingAmount.Value )
        {
            return Result<string>.Failure( "Ödeme tutarı kalan borçtan büyük olamaz." );
        }

        var paymentType = await paymentTypesRepository.FirstOrDefaultAsync(
           x => x.Id == request.PaymentTypeId,
           cancellationToken );

        if ( paymentType is null )
        {
            return Result<string>.Failure( "Ödeme tipi bulunamadı." );
        }

        if ( !paymentType.IsActive )
        {
            return Result<string>.Failure( "Pasif ödeme tipi ile ödeme alınamaz." );
        }

        if ( string.Equals(
            paymentType.PaymentType.Value,
            "NoPay",
            StringComparison.OrdinalIgnoreCase ) )
        {
            return Result<string>.Failure( "NoPay ile ödeme alınamaz." );
        }

        var newPaidAmount = customerBalance.PaidAmount.Value + request.PaymentAmount;
        var newOutstandingAmount = customerBalance.OutstandingAmount.Value - request.PaymentAmount;

        customerBalance.SetPaymentType( new IdentityId( request.PaymentTypeId ) );

        customerBalance.SetAmounts(
            totalAmount: new TotalAmount( customerBalance.TotalAmount.Value ),
            paidAmount: new PaidAmount( newPaidAmount ),
            outstandingAmount: new OutstandingAmount( newOutstandingAmount ) );

        customerBalance.SetLastPaymentAt( new LastPaymentAt( request.PaymentDate ) );

        customerBalanceRepository.Update( customerBalance );

        var paymentHistory = PaymentHistory.Create(
            customerId: customerBalance.CustomerId,
            sourceType: customerBalance.SourceType,
            sourceId: customerBalance.SourceId,
            paymentTypeId: new IdentityId( request.PaymentTypeId ),
            paymentAmount: request.PaymentAmount,
            remainingBalance: newOutstandingAmount,
            paymentDate: request.PaymentDate,
            notes: BuildPaymentNote( request.Notes )
            );

        paymentHistoryRepository.Add( paymentHistory );

        await unitOfWork.SaveChangesAsync( cancellationToken );
        return "Ödeme başarıyla alındı.";
    }

    private static string BuildPaymentNote( string? notes )
    {
        if ( string.IsNullOrWhiteSpace( notes ) )
        {
            return "Müşteri ekstresi üzerinden alınan ödeme.";
        }

        return $"Müşteri ekstresi üzerinden alınan ödeme. Not: {notes.Trim()}";
    }
}
using DeKayaServer.Application.Behaviors;
using DeKayaServer.Contracts.CustomerAccount;
using DeKayaServer.Domain.CustomerAccountAdjustments;
using DeKayaServer.Domain.CustomerAccountAdjustments.Enum;
using DeKayaServer.Domain.CustomerBalance;
using DeKayaServer.Domain.Customers;
using FluentValidation;
using GenericRepository;
using TS.MediatR;
using TS.Result;
using AccountAdjustmentAmount = DeKayaServer.Domain.CustomerAccountAdjustments.ValueObjects.AdjustmentAmount;
using AdjustmentNote = DeKayaServer.Domain.CustomerAccountAdjustments.ValueObjects.AdjustmentNote;
using AdjustmentReason = DeKayaServer.Domain.CustomerAccountAdjustments.ValueObjects.AdjustmentReason;
using BalanceAdjustmentAmount = DeKayaServer.Domain.CustomerBalance.ValueObjects.AdjustmentAmount;

namespace DeKayaServer.Application.Customers;

[Permission( "customer-account:discount-apply" )]
public sealed record ApplyCustomerBalanceDiscountCommand(
    Guid CustomerId,
    Guid CustomerBalanceId,
    decimal Amount,
    string Reason,
    string? Note ) : IRequest<Result<string>>
{
    public static ApplyCustomerBalanceDiscountCommand FromRequest(
        Guid customerId,
        Guid customerBalanceId,
        ApplyCustomerBalanceDiscountRequest request )
        => new(
            CustomerId: customerId,
            CustomerBalanceId: customerBalanceId,
            Amount: request.Amount,
            Reason: request.Reason,
            Note: request.Note );
}

public sealed class ApplyCustomerBalanceDiscountCommandValidator
    : AbstractValidator<ApplyCustomerBalanceDiscountCommand>
{
    public ApplyCustomerBalanceDiscountCommandValidator()
    {
        RuleFor( x => x.CustomerId )
            .NotEmpty()
            .WithMessage( "Geçerli bir müşteri seçin." );

        RuleFor( x => x.CustomerBalanceId )
            .NotEmpty()
            .WithMessage( "İndirim uygulanacak borç kalemi seçilmelidir." );

        RuleFor( x => x.Amount )
            .GreaterThan( 0 )
            .WithMessage( "İndirim tutarı 0'dan büyük olmalıdır." );

        RuleFor( x => x.Reason )
            .NotEmpty()
            .WithMessage( "İndirim sebebi zorunludur." )
            .MaximumLength( 250 )
            .WithMessage( "İndirim sebebi en fazla 250 karakter olabilir." );

        RuleFor( x => x.Note )
            .MaximumLength( 500 )
            .WithMessage( "Açıklama en fazla 500 karakter olabilir." );
    }
}

internal sealed class ApplyCustomerBalanceDiscountCommandHandler(
    ICustomerRepository customerRepository,
    ICustomerBalanceRepository customerBalanceRepository,
    ICustomerAccountAdjustmentRepository customerAccountAdjustmentRepository,
    IUnitOfWork unitOfWork )
    : IRequestHandler<ApplyCustomerBalanceDiscountCommand, Result<string>>
{
    public async Task<Result<string>> Handle(
        ApplyCustomerBalanceDiscountCommand request,
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

        if ( request.Amount > customerBalance.OutstandingAmount.Value )
        {
            return Result<string>.Failure( "İndirim tutarı kalan borçtan büyük olamaz." );
        }

        var balanceBeforeAdjustment = customerBalance.OutstandingAmount.Value;
        var balanceAfterAdjustment = balanceBeforeAdjustment - request.Amount;

        customerBalance.ApplyBalanceDecreaseAdjustment(
            new BalanceAdjustmentAmount( request.Amount ) );

        customerBalanceRepository.Update( customerBalance );

        var adjustment = CustomerAccountAdjustment.Create(
            customerId: customerBalance.CustomerId,
            customerBalanceId: customerBalance.Id,
            sourceType: customerBalance.SourceType,
            sourceId: customerBalance.SourceId,
            adjustmentType: CustomerAccountAdjustmentType.Discount,
            direction: CustomerAccountAdjustmentDirection.DecreaseBalance,
            amount: new AccountAdjustmentAmount( request.Amount ),
            reason: new AdjustmentReason( request.Reason.Trim() ),
            note: string.IsNullOrWhiteSpace( request.Note )
                ? null
                : new AdjustmentNote( request.Note.Trim() ),
            balanceBeforeAdjustment: balanceBeforeAdjustment,
            balanceAfterAdjustment: balanceAfterAdjustment );

        customerAccountAdjustmentRepository.Add( adjustment );

        await unitOfWork.SaveChangesAsync( cancellationToken );

        return "İndirim başarıyla uygulandı.";
    }
}

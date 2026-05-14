using DeKayaServer.Application.Behaviors;
using DeKayaServer.Application.Services;
using DeKayaServer.Contracts.CustomerAccount;
using DeKayaServer.Contracts.Pdf;
using TS.MediatR;
using TS.Result;

namespace DeKayaServer.Application.Customers;

[Permission( "customer-account:get" )]
public sealed record CustomerAccountStatementPdfGetQuery(
    Guid CustomerId,
    DateTime? StartDate,
    DateTime? EndDate )
    : IRequest<Result<PdfFileResult>>;

internal sealed class CustomerAccountStatementPdfGetQueryHandler(
    ICustomerAccountReaderService customerAccountReaderService,
    IPdfDocumentRenderer<CustomerAccountStatementPdfModel> pdfRenderer,
    IUserContext userContext )
    : IRequestHandler<CustomerAccountStatementPdfGetQuery, Result<PdfFileResult>>
{
    public async Task<Result<PdfFileResult>> Handle(
        CustomerAccountStatementPdfGetQuery request,
        CancellationToken cancellationToken )
    {
        var accountResult = await customerAccountReaderService.GetAccountAsync(
            customerId: request.CustomerId,
            startDate: request.StartDate,
            endDate: request.EndDate,
            cancellationToken: cancellationToken );

        if ( !accountResult.IsSuccessful || accountResult.Data is null )
        {
            return Result<PdfFileResult>.Failure(
                accountResult.ErrorMessages?.FirstOrDefault()
                ?? "Müşteri ekstresi oluşturulamadı." );
        }

        var account = accountResult.Data;
        var model = MapToPdfModel( account, userContext.GetFullName() );

        var pdf = pdfRenderer.Render( model );

        return Result<PdfFileResult>.Succeed( pdf );
    }

    private static CustomerAccountStatementPdfModel MapToPdfModel(
        CustomerAccountDto account,
        string generatedBy )
        => new()
        {
            DocumentInfo = new PdfDocumentInfoDto
            {
                Title = "Müşteri Ekstresi",
                Subtitle = FormatPeriod( account.PeriodStartDate, account.PeriodEndDate ),
                GeneratedAt = DateTime.Now,
                GeneratedBy = generatedBy
            },

            CustomerId = account.CustomerId,
            CustomerFullName = account.CustomerFullName,
            PhoneNumber = account.PhoneNumber,
            Email = account.Email,

            OpeningBalance = account.OpeningBalance,
            PeriodDebtAmount = account.PeriodDebtAmount,
            PeriodPaidAmount = account.PeriodPaidAmount,
            PeriodAdjustmentAmount = account.PeriodAdjustmentAmount,
            ClosingBalance = account.ClosingBalance,

            Lines = account.StatementItems
                .Select( x => new CustomerAccountStatementPdfLineModel
                {
                    TransactionDate = x.TransactionDate,
                    TransactionType = x.TransactionType,
                    SourceType = x.SourceType,
                    ReservationNumber = x.ReservationNumber,
                    Description = x.Description,
                    DebitAmount = x.DebitAmount,
                    CreditAmount = x.CreditAmount,
                    BalanceAfterTransaction = x.BalanceAfterTransaction
                } )
                .ToList()
        };

    private static string FormatPeriod(
        DateTime? startDate,
        DateTime? endDate )
    {
        if ( startDate is null && endDate is null )
        {
            return "Tüm dönem";
        }

        if ( startDate is not null && endDate is not null )
        {
            return $"{startDate:dd.MM.yyyy} - {endDate:dd.MM.yyyy}";
        }

        if ( startDate is not null )
        {
            return $"{startDate:dd.MM.yyyy} sonrası";
        }

        return $"{endDate:dd.MM.yyyy} öncesi";
    }
}

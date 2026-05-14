using DeKayaServer.Application.Services;
using DeKayaServer.Contracts.CustomerAccount;
using DeKayaServer.Infrastructure.Options;
using DeKayaServer.Infrastructure.Pdf.Shared;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace DeKayaServer.Infrastructure.Pdf.CustomerAccount;

internal sealed class CustomerAccountStatementPdfRenderer(
    IOptions<CompanyInformationOptions> companyOptions,
    IHostEnvironment hostEnvironment )
    : DeKayaPdfDocumentBase( companyOptions.Value, hostEnvironment ),
      IPdfDocumentRenderer<CustomerAccountStatementPdfModel>
{
    public PdfFileResult Render( CustomerAccountStatementPdfModel model )
    {
        var document = Document.Create( container =>
        {
            container.Page( page =>
            {
                ConfigurePage(
                    page,
                    model.DocumentInfo,
                    content =>
                    {
                        content.Column( column =>
                        {
                            column.Item()
                                .Element( item => ComposeCustomerInfo( item, model ) );

                            column.Item()
                                .PaddingTop( 10 )
                                .Element( item => ComposeSummary( item, model ) );

                            column.Item()
                                .PaddingTop( 12 )
                                .Element( item => ComposeStatementTable( item, model ) );

                            column.Item()
                                .PaddingTop( 10 )
                                .Element( item => ComposeStatementTotals( item, model ) );
                        } );
                    } );
            } );
        } );

        return new PdfFileResult
        {
            Content = document.GeneratePdf(),
            FileName = BuildFileName( model ),
            ContentType = "application/pdf"
        };
    }

    private static void ComposeCustomerInfo(
        IContainer container,
        CustomerAccountStatementPdfModel model )
    {
        container.Column( column =>
        {
            column.Item().Row( row =>
            {
                row.RelativeItem().Column( left =>
                {
                    left.Item()
                        .Text( "Müşteri" )
                        .FontSize( 7.5f )
                        .FontColor( DeKayaPdfTheme.MutedTextColor );

                    left.Item()
                        .PaddingTop( 3 )
                        .Text( model.CustomerFullName )
                        .FontSize( 12 )
                        .SemiBold()
                        .FontColor( DeKayaPdfTheme.TextColor );

                    left.Item()
                        .PaddingTop( 2 )
                        .Text( Display( model.PhoneNumber ) )
                        .FontSize( 8 )
                        .FontColor( DeKayaPdfTheme.MutedTextColor );
                } );

                row.RelativeItem().AlignBottom().Column( right =>
                {
                    right.Item()
                        .AlignRight()
                        .Text( Display( model.Email ) )
                        .FontSize( 8 )
                        .FontColor( DeKayaPdfTheme.MutedTextColor );

                    right.Item()
                        .PaddingTop( 2 )
                        .AlignRight()
                        .Text( Display( model.DocumentInfo.Subtitle ) )
                        .FontSize( 8 )
                        .FontColor( DeKayaPdfTheme.MutedTextColor );
                } );
            } );

            column.Item()
                .PaddingTop( 14 )
                .LineHorizontal( 1 )
                .LineColor( DeKayaPdfTheme.BorderColor );
        } );
    }



    private static void ComposeSummary(
        IContainer container,
        CustomerAccountStatementPdfModel model )
    {
        container.Row( row =>
        {
            AddSummaryBox( row, "Devir Bakiyesi", model.OpeningBalance );
            AddSummaryBox( row, "Dönem Borcu", model.PeriodDebtAmount );
            AddSummaryBox( row, "Dönem Ödemesi", model.PeriodPaidAmount );
            AddSummaryBox( row, "Dönem İndirimi", model.PeriodAdjustmentAmount );
            AddSummaryBox( row, "Dönem Sonu Bakiye", model.ClosingBalance );
        } );
    }

    private static void AddSummaryBox(
        RowDescriptor row,
        string title,
        decimal value )
    {
        row.RelativeItem()
            .PaddingRight( 8 )
            .BorderBottom( 1 )
            .BorderColor( DeKayaPdfTheme.BorderColor )
            .PaddingVertical( 10 )
            .Column( column =>
            {
                column.Item()
                    .Text( title )
                    .FontSize( 7.5f )
                    .FontColor( DeKayaPdfTheme.MutedTextColor );

                column.Item()
                    .PaddingTop( 4 )
                    .Text( FormatMoney( value ) )
                    .FontSize( 13 )
                    .SemiBold()
                    .FontColor( DeKayaPdfTheme.TextColor );
            } );
    }


    private static void ComposeStatementTable(
        IContainer container,
        CustomerAccountStatementPdfModel model )
    {
        container.Table( table =>
        {
            table.ColumnsDefinition( columns =>
            {
                columns.ConstantColumn( 56 );
                columns.ConstantColumn( 42 );
                columns.ConstantColumn( 72 );
                columns.RelativeColumn();
                columns.ConstantColumn( 58 );
                columns.ConstantColumn( 58 );
                columns.ConstantColumn( 58 );
                columns.ConstantColumn( 64 );
            } );

            table.Header( header =>
            {
                AddHeaderCell( header, "Tarih" );
                AddHeaderCell( header, "İşlem" );
                AddHeaderCell( header, "Kaynak" );
                AddHeaderCell( header, "Açıklama" );
                AddHeaderCell( header, "Borç" );
                AddHeaderCell( header, "Ödeme" );
                AddHeaderCell( header, "İndirim" );
                AddHeaderCell( header, "Bakiye" );
            } );

            foreach ( var line in model.Lines )
            {
                AddBodyCell( table, line.TransactionDate.ToString( "dd.MM.yyyy" ) );
                AddBodyCell( table, line.TransactionType );
                AddBodyCell( table, FormatSource( line ) );
                AddBodyCell( table, Display( line.Description ) );
                AddMoneyCell( table, line.DebitAmount );
                AddMoneyCell( table, GetPaymentAmount( line ) );
                AddMoneyCell( table, GetAdjustmentAmount( line ) );
                AddMoneyCell( table, line.BalanceAfterTransaction );
            }
        } );
    }

    private static void ComposeStatementTotals(
    IContainer container,
    CustomerAccountStatementPdfModel model )
    {
        container.AlignRight()
            .Width( 210 )
            .Column( column =>
            {
                AddTotalLine( column, "Toplam Borç", model.PeriodDebtAmount, DeKayaPdfTheme.TextColor );
                AddTotalLine( column, "Yapılan Ödeme", model.PeriodPaidAmount, DeKayaPdfTheme.TextColor );
                AddTotalLine( column, "Yapılan İndirim", model.PeriodAdjustmentAmount, DeKayaPdfTheme.TextColor );

                column.Item()
                    .PaddingTop( 5 )
                    .LineHorizontal( 1 )
                    .LineColor( DeKayaPdfTheme.BorderColor );

                AddTotalLine( column, "Kalan Borç", model.ClosingBalance, DeKayaPdfTheme.TextColor, isStrong: true );
            } );
    }

    private static void AddTotalLine(
        ColumnDescriptor column,
        string label,
        decimal value,
        string color,
        bool isStrong = false )
    {
        column.Item()
            .PaddingVertical( 2 )
            .Row( row =>
            {
                row.RelativeItem()
                    .Text( label )
                    .FontSize( isStrong ? 8.5f : 8 )
                    .SemiBold();

                row.ConstantItem( 86 )
                    .AlignRight()
                    .Text( FormatMoney( value ) )
                    .FontSize( isStrong ? 8.5f : 8 )
                    .SemiBold()
                    .FontColor( color );
            } );
    }


    private static void AddHeaderCell(
        TableCellDescriptor cell,
        string text )
    {
        cell.Cell()
            .BorderBottom( 1 )
            .BorderColor( DeKayaPdfTheme.TextColor )
            .PaddingVertical( 6 )
            .PaddingHorizontal( 4 )
            .Text( text )
            .FontColor( DeKayaPdfTheme.TextColor )
            .SemiBold()
            .FontSize( 8 );
    }

    private static void AddBodyCell(
        TableDescriptor table,
        string text )
    {
        table.Cell()
            .BorderBottom( 1 )
            .BorderColor( DeKayaPdfTheme.BorderColor )
            .PaddingVertical( 4 )
            .PaddingHorizontal( 4 )
            .Text( text )
            .FontSize( 8 );
    }

    private static void AddMoneyCell(
        TableDescriptor table,
        decimal value )
    {
        table.Cell()
            .BorderBottom( 1 )
            .BorderColor( DeKayaPdfTheme.BorderColor )
            .PaddingVertical( 4 )
            .PaddingHorizontal( 4 )
            .AlignRight()
            .Text( value == 0 ? "-" : FormatMoney( value ) )
            .FontSize( 8 );
    }

    private static string BuildFileName(
        CustomerAccountStatementPdfModel model )
    {
        var safeCustomerName = string.Join(
            "-",
            model.CustomerFullName
                .Split( Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries ) );

        return $"musteri-ekstresi-{safeCustomerName}-{DateTime.Now:yyyyMMddHHmm}.pdf";
    }

    private static string FormatSource(
        CustomerAccountStatementPdfLineModel line )
    {
        var sourceType = line.SourceType switch
        {
            "Reservation" => "Rezervasyon",
            "Product" => "Ürün",
            "Service" => "Hizmet",
            _ => line.SourceType
        };

        if ( string.IsNullOrWhiteSpace( line.ReservationNumber ) )
        {
            return sourceType;
        }

        return $"{sourceType} - {line.ReservationNumber}";
    }

    private static string FormatMoney( decimal value )
        => value.ToString( "N2" ) + " ₺";

    private static string Display( string? value )
        => string.IsNullOrWhiteSpace( value ) ? "-" : value;

    private static decimal GetPaymentAmount(
    CustomerAccountStatementPdfLineModel line )
    => line.TransactionType == "Ödeme"
        ? line.CreditAmount
        : 0;

    private static decimal GetAdjustmentAmount(
        CustomerAccountStatementPdfLineModel line )
        => line.TransactionType is "İndirim" or "Mahsup" or "Alacak kapama"
            ? line.CreditAmount
            : 0;

}

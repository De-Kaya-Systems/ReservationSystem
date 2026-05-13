using DeKayaServer.Contracts.Pdf;
using DeKayaServer.Infrastructure.Options;
using Microsoft.Extensions.Hosting;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace DeKayaServer.Infrastructure.Pdf.Shared;

internal abstract class DeKayaPdfDocumentBase(
    CompanyInformationOptions companyOptions,
    IHostEnvironment hostEnvironment )
{
    protected CompanyInformationOptions CompanyOptions { get; } = companyOptions;
    private IHostEnvironment HostEnvironment { get; } = hostEnvironment;

    protected void ConfigurePage(
        PageDescriptor page,
        PdfDocumentInfoDto documentInfo,
        Action<IContainer> content )
    {
        page.Size( PageSizes.A4 );
        page.Margin( 34 );
        page.DefaultTextStyle( x => x
            .FontFamily( DeKayaPdfTheme.FontFamily )
            .FontSize( 9 )
            .FontColor( DeKayaPdfTheme.TextColor ) );

        page.Header().Element( header => ComposeHeader( header, documentInfo ) );
        page.Content().PaddingVertical( 12 ).Element( content );
        page.Footer().Element( ComposeFooter );
    }

    private void ComposeHeader(
        IContainer container,
        PdfDocumentInfoDto documentInfo )
    {
        container.Column( column =>
        {
            column.Item().Row( row =>
            {
                row.RelativeItem().Column( company =>
                {
                    var logoPath = ResolveLogoPath();

                    if ( logoPath is not null )
                    {
                        company.Item()
                            .Width( 116 )
                            .Image( logoPath )
                            .FitWidth();

                        company.Item().PaddingTop( 8 );
                    }

                    company.Item()
                        .Text( CompanyOptions.CompanyName )
                        .FontSize( 10.5f )
                        .SemiBold()
                        .FontColor( DeKayaPdfTheme.TextColor );

                    if ( !string.IsNullOrWhiteSpace( CompanyOptions.Address ) )
                    {
                        company.Item()
                            .PaddingTop( 3 )
                            .Text( CompanyOptions.Address )
                            .FontSize( 7.5f )
                            .FontColor( DeKayaPdfTheme.MutedTextColor );
                    }

                    var contactLine = BuildContactLine();

                    if ( !string.IsNullOrWhiteSpace( contactLine ) )
                    {
                        company.Item()
                            .PaddingTop( 2 )
                            .Text( contactLine )
                            .FontSize( 7.5f )
                            .FontColor( DeKayaPdfTheme.MutedTextColor );
                    }
                } );

                row.ConstantItem( 190 )
                    .AlignRight()
                    .Column( document =>
                    {
                        document.Item()
                            .AlignRight()
                            .Text( documentInfo.Title )
                            .FontSize( 16 )
                            .SemiBold()
                            .FontColor( DeKayaPdfTheme.TextColor );

                        if ( !string.IsNullOrWhiteSpace( documentInfo.Subtitle ) )
                        {
                            document.Item()
                                .PaddingTop( 4 )
                                .AlignRight()
                                .Text( documentInfo.Subtitle )
                                .FontSize( 8.5f )
                                .FontColor( DeKayaPdfTheme.MutedTextColor );
                        }

                        document.Item()
                            .PaddingTop( 8 )
                            .AlignRight()
                            .Text( $"Oluşturulma: {documentInfo.GeneratedAt:dd.MM.yyyy HH:mm}" )
                            .FontSize( 7.5f )
                            .FontColor( DeKayaPdfTheme.MutedTextColor );

                        document.Item()
                            .AlignRight()
                            .Text( $"Oluşturan: {documentInfo.GeneratedBy}" )
                            .FontSize( 7.5f )
                            .FontColor( DeKayaPdfTheme.MutedTextColor );
                    } );
            } );

            column.Item()
                .PaddingTop( 18 )
                .LineHorizontal( 1 )
                .LineColor( DeKayaPdfTheme.BorderColor );
        } );
    }

    private void ComposeFooter( IContainer container )
    {
        container.Column( column =>
        {
            column.Item()
                .LineHorizontal( 0.8f )
                .LineColor( DeKayaPdfTheme.BorderColor );

            column.Item()
                .PaddingTop( 8 )
                .Row( row =>
                {
                    row.RelativeItem()
                        .Text( CompanyOptions.Website ?? "www.de-kaya.com" )
                        .FontSize( 7.5f )
                        .FontColor( DeKayaPdfTheme.MutedTextColor );

                    row.ConstantItem( 120 )
                        .AlignRight()
                        .Text( text =>
                        {
                            text.Span( "Sayfa " ).FontSize( 7.5f );
                            text.CurrentPageNumber().FontSize( 7.5f );
                            text.Span( " / " ).FontSize( 7.5f );
                            text.TotalPages().FontSize( 7.5f );
                        } );
                } );
        } );
    }

    private string? BuildContactLine()
    {
        var parts = new List<string>();

        if ( !string.IsNullOrWhiteSpace( CompanyOptions.Phone ) )
        {
            parts.Add( CompanyOptions.Phone );
        }

        if ( !string.IsNullOrWhiteSpace( CompanyOptions.Email ) )
        {
            parts.Add( CompanyOptions.Email );
        }

        if ( !string.IsNullOrWhiteSpace( CompanyOptions.Website ) )
        {
            parts.Add( CompanyOptions.Website );
        }

        return parts.Count == 0
            ? null
            : string.Join( " | ", parts );
    }

    private string? ResolveLogoPath()
    {
        if ( string.IsNullOrWhiteSpace( CompanyOptions.LogoPath ) )
        {
            return null;
        }

        var configuredPath = CompanyOptions.LogoPath.Replace( '/', Path.DirectorySeparatorChar );

        var fullPath = Path.IsPathRooted( configuredPath )
            ? configuredPath
            : Path.Combine( HostEnvironment.ContentRootPath, configuredPath );

        return File.Exists( fullPath )
            ? fullPath
            : null;
    }
}

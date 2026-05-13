namespace DeKayaServer.Application.Services;

public interface IPdfDocumentRenderer<in TModel>
{
    PdfFileResult Render( TModel model );
}

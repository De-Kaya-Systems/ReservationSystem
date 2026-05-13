namespace DeKayaServer.Application.Services;

public sealed class PdfFileResult
{
    public byte[] Content { get; set; } = [];
    public string FileName { get; set; } = default!;
    public string ContentType { get; set; } = "application/pdf";
}
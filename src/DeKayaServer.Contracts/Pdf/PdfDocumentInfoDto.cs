namespace DeKayaServer.Contracts.Pdf;

public sealed class PdfDocumentInfoDto
{
    public string Title { get; set; } = default!;
    public string? Subtitle { get; set; }
    public DateTime GeneratedAt { get; set; }
    public string GeneratedBy { get; set; } = default!;
}

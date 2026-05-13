namespace DeKayaServer.Infrastructure.Options;

public sealed class CompanyInformationOptions
{
    public const string SectionName = "CompanyInformation";

    public string CompanyName { get; set; } = "De-Kaya Bilgi Tek. ve Soğutma Sistemleri";
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public string? LogoPath { get; set; }
}
namespace DeKayaServer.BlazorApp.ViewModels;

public sealed class CustomerViewModel
{
    public Guid Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? FullName { get; set; }
    public string? AddressCity { get; set; }
    public string? AddressDistrict { get; set; }
    public string? AddressFull { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactPhone2 { get; set; }
    public string? ContactEmail { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? UpdatedFullName { get; set; }
    public string? CreatedFullName { get; set; }
}

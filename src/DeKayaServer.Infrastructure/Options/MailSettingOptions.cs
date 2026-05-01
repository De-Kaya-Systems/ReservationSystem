namespace DeKayaServer.Infrastructure.Options;

public sealed class MailSettingOptions
{
    public string ConnectionString { get; set; } = default!;
    public string SenderAddress { get; set; } = default!;
}

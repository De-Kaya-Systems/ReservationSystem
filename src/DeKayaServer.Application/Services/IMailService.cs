namespace DeKayaServer.Application.Services;

public interface IMailService
{
    Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default);
}

using DeKayaServer.Application.Services;
using FluentEmail.Core;

namespace DeKayaServer.Infrastructure.Services;

/// <summary>
/// FluentEmail.Smtp kütüphanesi kullanıyoruz.
/// </summary>
internal sealed class MailService(
    IFluentEmail fluentEmail) : IMailService
{
    public async Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        var sendResponse = await fluentEmail
            .To(to)
            .Subject(subject)
            .Body(body, isHtml: true)
            .SendAsync(cancellationToken);

        if (!sendResponse.Successful)
        {
            // Hata mesajlarını birleştirip fırlatıyoruz.
            // EN: We concatenate error messages and throw.
            throw new ArgumentException(string.Join(", ", sendResponse.ErrorMessages));
        }
    }
}

using Azure;
using Azure.Communication.Email;
using DeKayaServer.Application.Services;
using DeKayaServer.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace DeKayaServer.Infrastructure.Services;

internal sealed class MailService(
    EmailClient emailClient,
    IOptions<MailSettingOptions> mailSettingOptions ) : IMailService
{
    public async Task SendAsync( string to, string subject, string body, CancellationToken cancellationToken = default )
    {
        cancellationToken.ThrowIfCancellationRequested();

        var options = mailSettingOptions.Value;
        if ( string.IsNullOrWhiteSpace( options.ConnectionString ) )
        {
            throw new InvalidOperationException( "Mail service configuration is missing: MailSettings:ConnectionString." );
        }

        if ( string.IsNullOrWhiteSpace( options.SenderAddress ) )
        {
            throw new InvalidOperationException( "Mail service configuration is missing: MailSettings:SenderAddress." );
        }

        var emailMessage = new EmailMessage(
            senderAddress: options.SenderAddress,
            content: new EmailContent( subject )
            {
                PlainText = "Bu e-posta HTML içerik barındırır. Lütfen HTML destekli bir istemci ile görüntüleyin.",
                Html = body
            },
            recipients: new EmailRecipients(
            [
                new EmailAddress(to)
            ] ) );

        EmailSendOperation emailSendOperation;
        try
        {
            using var sendTimeoutCts = new CancellationTokenSource( TimeSpan.FromSeconds( 45 ) );
            emailSendOperation = await emailClient.SendAsync(
                WaitUntil.Started,
                emailMessage,
                sendTimeoutCts.Token );
        }
        catch ( TaskCanceledException ex )
        {
            throw new InvalidOperationException(
                "Azure Communication Email send timed out before request was accepted.",
                ex );
        }
        catch ( RequestFailedException ex )
        {
            throw new InvalidOperationException(
                $"Azure Communication Email send failed. Status: {ex.Status}, Code: {ex.ErrorCode}, Message: {ex.Message}",
                ex );
        }

        await emailSendOperation.UpdateStatusAsync( cancellationToken );
        if ( !emailSendOperation.HasCompleted )
        {
            return;
        }

        var operationStatus = emailSendOperation.Value.Status;
        if ( operationStatus != EmailSendStatus.Succeeded )
        {
            throw new InvalidOperationException(
                $"Email could not be sent. Status: {operationStatus}" );
        }
    }
}

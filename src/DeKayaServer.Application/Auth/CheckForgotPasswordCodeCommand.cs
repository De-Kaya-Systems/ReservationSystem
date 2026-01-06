using DeKayaServer.Domain.Users;
using TS.MediatR;
using TS.Result;

namespace DeKayaServer.Application.Auth;

public sealed record CheckForgotPasswordCodeCommand(
    Guid ForgotPasswordCode) : IRequest<Result<bool>>;

internal sealed class CheckForgotPasswordCodeCommandHandler(
    IUserRepository userRepository) : IRequestHandler<CheckForgotPasswordCodeCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(CheckForgotPasswordCodeCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.FirstOrDefaultAsync(p =>
               p.ForgotPasswordCode != null
               && p.ForgotPasswordCode.Value == request.ForgotPasswordCode
               && p.IsForgotPasswordCompleted.Value == false, cancellationToken);

        if (user == null)
        {
            return Result<bool>.Failure("Geçersiz istek!");
        }

        var fpDate = user.ForgotPasswordDate!.Value.AddDays(1);
        var now = DateTimeOffset.Now;
        if (fpDate < now)
        {
            return Result<bool>.Failure("Şifre sıfırlama isteğinizin süresi dolmuştur. Lütfen tekrar deneyin!");
        }

        return true;
    }
}
using DeKayaServer.Domain.Users;
using DeKayaServer.Domain.Users.ValueObjects;
using FluentValidation;
using GenericRepository;
using TS.MediatR;
using TS.Result;

namespace DeKayaServer.Application.Auth;

public sealed record ResetPasswordCommand(Guid ForgotPasswordCode, string NewPassword) : IRequest<Result<string>>;

public sealed class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("Yeni şifre girin!");
    }
}

internal sealed class ResetPasswordCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<ResetPasswordCommand, Result<string>>
{
    public async Task<Result<string>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.FirstOrDefaultAsync(p =>
        p.ForgotPasswordCode != null
        && p.ForgotPasswordCode.Value == request.ForgotPasswordCode
        && p.IsForgotPasswordCompleted.Value == false, cancellationToken);

        if (user == null)
        {
            return Result<string>.Failure("Geçersiz istek!");
        }

        var fpDate = user.ForgotPasswordDate!.Value.AddDays(1);
        var now = DateTimeOffset.Now;
        if (fpDate < now)
        {
            return Result<string>.Failure("Şifre sıfırlama isteğinizin süresi dolmuştur. Lütfen tekrar deneyin!");
        }

        Password password = new(request.NewPassword);
        user.SetPassword(password);
        userRepository.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return "Şifreniz başarıyla sıfırlandı.";
    }
}
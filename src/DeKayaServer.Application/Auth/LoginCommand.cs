using DeKayaServer.Application.Services;
using DeKayaServer.Domain.Users;
using FluentValidation;
using TS.MediatR;
using TS.Result;

namespace DeKayaServer.Application.Auth;

public sealed record LoginCommand(
    string EmailOrUserName,
    string Password) : IRequest<Result<string>>;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.EmailOrUserName).NotEmpty().WithMessage("Username or email is required.");
        RuleFor(x => x.Password).NotEmpty().WithMessage("Password is required.");
    }
}

public sealed class LoginCommandHandler(
    IUserRepository userRepository,
    IJwtProvider jwtProvider) : IRequestHandler<LoginCommand, Result<string>>
{
    public async Task<Result<string>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.FirstOrDefaultAsync(u =>
            u.Email.Value == request.EmailOrUserName
            || u.UserName.Value == request.EmailOrUserName);

        if (user is null)
        {
            return Result<string>.Failure("Invalid username or email. / Kullanıcı adı veya şifre hatalı!");
        }

        var checkPassword = user.VerifyPasswordHash(request.Password);
        if (!checkPassword)
        {
            return Result<string>.Failure("Invalid password / Şifre Hatalı!");
        }

        var token = jwtProvider.CreateToken(user);
        return token;
    }
}
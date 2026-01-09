using DeKayaServer.Application.Services;
using DeKayaServer.Domain.LoginTokens;
using DeKayaServer.Domain.LoginTokens.ValueObjects;
using DeKayaServer.Domain.Users;
using DeKayaServer.Infrastructure.Options;
using GenericRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DeKayaServer.Infrastructure.Services;

internal sealed class JwtProvider(
    ILoginTokenRepository loginTokenRepository,
    IUnitOfWork unitOfWork,
    IOptions<JwtOptions> options) : IJwtProvider
{
    public async Task<string> CreateTokenAsync(User user, CancellationToken cancellationToken = default)
    {
        List<Claim> claims = new()
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim("firstName", user.FirstName.Value),
            new Claim("lastName", user.LastName.Value),
            new Claim("fullName", user.FullName.Value),
            new Claim("email", user.Email.Value),
        };

        SymmetricSecurityKey securityKey = new(Encoding.UTF8.GetBytes(options.Value.SecretKey));
        SigningCredentials signingCredentials = new(securityKey, SecurityAlgorithms.HmacSha512);

        var ExpiresDate = DateTime.Now.AddDays(1);

        JwtSecurityToken securityToken = new(
            issuer: options.Value.Issuer,
            audience: options.Value.Audience,
            claims: claims,
            notBefore: DateTime.Now,
            expires: ExpiresDate,
            signingCredentials: signingCredentials);

        var handler = new JwtSecurityTokenHandler();
        var token = handler.WriteToken(securityToken);

        //Yeni bir aktif token oluşturacak ve veritabanına ekleyecek.
        //EN: It will create a new active token and add it to the database.
        Token newToken = new(token);
        ExpiresDate expiresDate = new(ExpiresDate);
        LoginToken loginToken = new(newToken, user.Id, expiresDate);
        loginTokenRepository.Add(loginToken);

        //Eski tokenları pasif yapacak.Böylece tek token geçerli olacak.
        //EN: It will deactivate old tokens. Thus, only one token will be valid.
        var loginTokens = await loginTokenRepository
            .Where(x => x.UserId == user.Id && x.IsActive.Value == true)
            .ToListAsync(cancellationToken);

        foreach (var item in loginTokens)
        {
            item.SetIsActive(new(false));
        }

        loginTokenRepository.UpdateRange(loginTokens);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return token;
    }
}

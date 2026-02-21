using DeKayaServer.Contracts.Users;
using TS.Result;

namespace DeKayaServer.BlazorApp.Interfaces;

public interface IUserService
{
    Task<Result<string>> CreateAsync(
       string firstName,
       string lastName,
       string email,
       string userName,
       Guid roleId,
       CancellationToken cancellationToken = default );

    Task<Result<string>> UpdateAsync(
        Guid id,
        string firstName,
        string lastName,
        string email,
        string userName,
        Guid roleId,
        CancellationToken cancellationToken = default );

    Task<Result<string>> DeleteAsync( Guid id, CancellationToken cancellationToken = default );

    Task<Result<UserDto>> GetByIdAsync( Guid id, CancellationToken cancellationToken = default );

    Task<Result<List<UserDto>>> GetAllAsync( CancellationToken cancellationToken = default );
}

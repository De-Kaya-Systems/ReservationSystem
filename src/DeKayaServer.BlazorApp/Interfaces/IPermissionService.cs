using TS.Result;

namespace DeKayaServer.BlazorApp.Interfaces;

public interface IPermissionService
{
    Task<Result<List<string>>> GetAllAsync( CancellationToken ct = default );
}
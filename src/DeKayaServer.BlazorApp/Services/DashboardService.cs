using DeKayaServer.BlazorApp.Http;
using DeKayaServer.Contracts.Dashboard;
using TS.Result;

namespace DeKayaServer.BlazorApp.Services;

public interface IDashboardService
{
    Task<Result<DashboardSummaryDto>> GetSummaryAsync(
        CancellationToken cancellationToken = default );
}

public sealed class DashboardService(
    IApiClient apiClient )
    : IDashboardService
{
    public Task<Result<DashboardSummaryDto>> GetSummaryAsync(
        CancellationToken cancellationToken = default )
        => apiClient.GetAsync<DashboardSummaryDto>(
            "dashboard/summary",
            cancellationToken );
}

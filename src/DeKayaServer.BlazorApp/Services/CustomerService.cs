using DeKayaServer.BlazorApp.Constants;
using DeKayaServer.BlazorApp.Http;
using DeKayaServer.Contracts.Common;
using DeKayaServer.Contracts.CustomerAccount;
using DeKayaServer.Contracts.Customers;
using TS.Result;

namespace DeKayaServer.BlazorApp.Services;

public interface ICustomerService
{
    Task<Result<CustomerAccountDto>> GetAccountAsync( Guid customerId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default );
    Task<Result<string>> CreateAsync( CreateCustomerRequest request, CancellationToken cancellationToken = default );
    Task<Result<string>> UpdateAsync( Guid Id, UpdateCustomerRequest request, CancellationToken cancellationToken = default );
    Task<Result<CustomerDto>> GetByIdAsync( Guid id, CancellationToken cancellationToken = default );
    Task<Result<List<CustomerDto>>> GetAllAsync( CancellationToken cancellationToken = default );
    Task<Result<string>> DeleteAsync( Guid id, CancellationToken cancellationToken = default );
    Task<Result<PagedResultDto<CustomerListItemDto>>> GetListAsync(
        string? customerName,
        string? phoneNumber,
        int pageIndex,
        int pageSize,
        CancellationToken cancellationToken = default );
}

public class CustomerService( IApiClient apiClient ) : ICustomerService
{
    public Task<Result<PagedResultDto<CustomerListItemDto>>> GetListAsync(
        string? customerName,
        string? phoneNumber,
        int pageIndex,
        int pageSize,
        CancellationToken cancellationToken = default )
    {
        var query = new List<string>
        {
            $"pageIndex={pageIndex}",
            $"pageSize={pageSize}"
        };

        if ( !string.IsNullOrWhiteSpace( customerName ) )
        {
            query.Add( $"customerName={Uri.EscapeDataString( customerName.Trim() )}" );
        }

        if ( !string.IsNullOrWhiteSpace( phoneNumber ) )
        {
            query.Add( $"phoneNumber={Uri.EscapeDataString( phoneNumber.Trim() )}" );
        }

        var url = $"{EndpointConstants.Customers}/list?{string.Join( "&", query )}";

        return apiClient.GetAsync<PagedResultDto<CustomerListItemDto>>( url, cancellationToken );
    }

    public Task<Result<CustomerAccountDto>> GetAccountAsync(
        Guid customerId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default )
    {
        var query = new List<string>();

        if ( startDate.HasValue )
        {
            query.Add( $"startDate={startDate.Value:yyyy-MM-dd}" );
        }

        if ( endDate.HasValue )
        {
            query.Add( $"endDate={endDate.Value:yyyy-MM-dd}" );
        }

        var url = $"{EndpointConstants.Customers}/{customerId}/account";

        if ( query.Count > 0 )
        {
            url += $"?{string.Join( "&", query )}";
        }

        return apiClient.GetAsync<CustomerAccountDto>( url, cancellationToken );
    }

    public Task<Result<string>> CreateAsync( CreateCustomerRequest request, CancellationToken cancellationToken = default )
        => apiClient.PostAsync<CreateCustomerRequest, string>(
            EndpointConstants.Customers,
            request,
            cancellationToken );
    public Task<Result<string>> UpdateAsync( Guid Id, UpdateCustomerRequest request, CancellationToken cancellationToken = default )
        => apiClient.PutAsync<UpdateCustomerRequest, string>(
            EndpointConstants.Customers,
            request,
            cancellationToken );

    public Task<Result<CustomerDto>> GetByIdAsync( Guid id, CancellationToken cancellationToken = default )
        => apiClient.GetAsync<CustomerDto>( $"{EndpointConstants.Customers}/{id}", cancellationToken );

    public async Task<Result<List<CustomerDto>>> GetAllAsync( CancellationToken cancellationToken = default )
    {
        var odataRes = await apiClient.GetRawAsync<ODataEnvelope<CustomerDto>>( EndpointConstants.ODataCustomers, cancellationToken );

        if ( !odataRes.IsSuccessful || odataRes.Data is null )
        {
            return new Result<List<CustomerDto>>
            {
                IsSuccessful = false,
                StatusCode = odataRes.StatusCode,
                ErrorMessages = odataRes.ErrorMessages
            };
        }

        return new Result<List<CustomerDto>>
        {
            IsSuccessful = true,
            StatusCode = odataRes.StatusCode,
            Data = odataRes.Data.Value ?? []
        };
    }

    public Task<Result<string>> DeleteAsync( Guid id, CancellationToken cancellationToken = default )
        => apiClient.DeleteAsync<string>( $"{EndpointConstants.Customers}/{id}", cancellationToken );

    private sealed class ODataEnvelope<T>
    {
        public List<T> Value { get; set; } = [];
    }
}

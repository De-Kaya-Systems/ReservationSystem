using DeKayaServer.BlazorApp.Constants;
using DeKayaServer.BlazorApp.Http;
using DeKayaServer.Contracts.Customers;
using TS.Result;

namespace DeKayaServer.BlazorApp.Services;

public interface ICustomerService
{
    Task<Result<string>> CreateAsync(
        string firstName,
        string lastName,
        string city,
        string district,
        string fullAddress,
        string phoneNumber,
        string phoneNumber2,
        string email,
        CancellationToken cancellationToken = default );

    Task<Result<CustomerDto>> GetByIdAsync( Guid id, CancellationToken cancellationToken = default );

    Task<Result<List<CustomerDto>>> GetAllAsync( CancellationToken cancellationToken = default );
}

public class CustomerService( IApiClient apiClient ) : ICustomerService
{
    public Task<Result<string>> CreateAsync(
        string firstName,
        string lastName,
        string city,
        string district,
        string fullAddress,
        string phoneNumber,
        string phoneNumber2,
        string email,
        CancellationToken cancellationToken = default )
        => apiClient.PostAsync<CreateCustomerRequest, string>(
            EndpointConstants.Customers,
            new CreateCustomerRequest(
                firstName,
                lastName,
                city,
                district,
                fullAddress,
                phoneNumber,
                phoneNumber2,
                email ),
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

    private sealed class ODataEnvelope<T>
    {
        public List<T> Value { get; set; } = [];
    }
}

using DeKayaServer.BlazorApp.Constants;
using DeKayaServer.BlazorApp.Http;
using DeKayaServer.Contracts.PaymentTypes;
using TS.Result;

namespace DeKayaServer.BlazorApp.Services;

public interface IPaymentTypeService
{
    Task<Result<List<PaymentTypeDto>>> GetAllAsync( CancellationToken cancellationToken = default );
}

public sealed class PaymentTypeService( IApiClient apiClient ) : IPaymentTypeService
{
    public async Task<Result<List<PaymentTypeDto>>> GetAllAsync( CancellationToken cancellationToken = default )
    {
        var odataRes = await apiClient.GetRawAsync<ODataEnvelope<PaymentTypeDto>>( EndpointConstants.ODataPaymentTypes, cancellationToken );

        if ( !odataRes.IsSuccessful || odataRes.Data is null )
        {
            return new Result<List<PaymentTypeDto>>
            {
                IsSuccessful = false,
                StatusCode = odataRes.StatusCode,
                ErrorMessages = odataRes.ErrorMessages
            };
        }

        return new Result<List<PaymentTypeDto>>
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
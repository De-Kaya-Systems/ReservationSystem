using Microsoft.AspNetCore.Components.Server.Circuits;

namespace DeKayaServer.BlazorApp.Http.TokenProcess;

public sealed class CircuitServicesAccessorCircuitHandler(
    IServiceProvider services,
    CircuitServicesAccessor accessor) : CircuitHandler
{
    public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        accessor.Set(circuit.Id, services);
        return Task.CompletedTask;
    }

    public override Task OnConnectionUpAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        accessor.Set(circuit.Id, services);
        return Task.CompletedTask;
    }

    public override Task OnConnectionDownAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        accessor.Remove(circuit.Id);
        return Task.CompletedTask;
    }

    public override Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        accessor.Remove(circuit.Id);
        return Task.CompletedTask;
    }
}

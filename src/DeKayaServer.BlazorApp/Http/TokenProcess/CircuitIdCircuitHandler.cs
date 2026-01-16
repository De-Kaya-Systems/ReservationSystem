using Microsoft.AspNetCore.Components.Server.Circuits;

namespace DeKayaServer.BlazorApp.Http.TokenProcess;

public sealed class CircuitIdCircuitHandler(
    CircuitIdProvider circuitIdProvider) : CircuitHandler
{
    public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        circuitIdProvider.CircuitId = circuit.Id;
        return Task.CompletedTask;
    }

    public override Task OnConnectionUpAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        circuitIdProvider.CircuitId = circuit.Id;
        return Task.CompletedTask;
    }
}

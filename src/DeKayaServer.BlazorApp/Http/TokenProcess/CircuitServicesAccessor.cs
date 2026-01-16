using System.Collections.Concurrent;

namespace DeKayaServer.BlazorApp.Http.TokenProcess;

public sealed class CircuitServicesAccessor
{
    private readonly ConcurrentDictionary<string, IServiceProvider> _byCircuitId = new(StringComparer.Ordinal);

    public bool TryGetServices(string circuitId, out IServiceProvider services)
        => _byCircuitId.TryGetValue(circuitId, out services!);

    public void Set(string circuitId, IServiceProvider services)
        => _byCircuitId[circuitId] = services;

    public void Remove(string circuitId)
        => _byCircuitId.TryRemove(circuitId, out _);
}

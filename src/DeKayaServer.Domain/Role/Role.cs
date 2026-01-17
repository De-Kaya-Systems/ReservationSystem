using DeKayaServer.Domain.Abstractions;
using DeKayaServer.Domain.Shared;

namespace DeKayaServer.Domain.Role;

public sealed class Role : Entity
{
    public Role() { }
    public Role(Name name, bool isActive)
    {
        SetName(name);
        SetStatus(isActive);
    }
    public Name Name { get; private set; } = default!;

    //Behaviors
    public void SetName(Name name) { Name = name; }
}

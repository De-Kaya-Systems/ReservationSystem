using DeKayaServer.Domain.Abstractions;
using DeKayaServer.Domain.Shared;

namespace DeKayaServer.Domain.Role;

public sealed class Role : Entity
{
    private readonly List<Permission> _permissions = new();
    public Role() { }
    public Role( Name name, bool isActive )
    {
        SetName( name );
        SetStatus( isActive );
    }
    public Name Name { get; private set; } = default!;
    public IReadOnlyCollection<Permission> Permissions => _permissions;

    #region Behaviors

    public void SetName( Name name ) { Name = name; }

    public void SetPermissions( IEnumerable<Permission> permissions )
    {
        _permissions.Clear();
        _permissions.AddRange( permissions );
    }

    #endregion
}

public sealed record Permission( string Value );
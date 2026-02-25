using DeKayaServer.Domain.Abstractions;
using DeKayaServer.Domain.Customers.ValueObjects;
using DeKayaServer.Domain.Shared;

namespace DeKayaServer.Domain.Customers;

public sealed class Customer : Entity
{
    private Customer() { }

    public Customer(
        FirstName firstName,
        LastName lastName,
        Address address,
        Contact contact )
    {
        SetFirstName( firstName );
        SetLastName( lastName );
        SetFullName();
        SetAddress( address );
        SetContact( contact );
    }

    public FirstName FirstName { get; private set; } = default!;
    public LastName LastName { get; private set; } = default!;
    public FullName FullName { get; private set; } = default!;
    public Address Address { get; private set; } = default!;
    public Contact Contact { get; private set; } = default!;

    #region Behaviors
    public void SetFirstName( FirstName firstName )
    {
        FirstName = firstName;
    }

    public void SetLastName( LastName lastName )
    {
        LastName = lastName;
    }

    public void SetFullName()
    {
        FullName = new( FirstName.Value + " " + LastName.Value );
    }

    public void SetAddress( Address address )
    {
        Address = address;
    }

    public void SetContact( Contact contact )
    {
        Contact = contact;
    }

    #endregion
}

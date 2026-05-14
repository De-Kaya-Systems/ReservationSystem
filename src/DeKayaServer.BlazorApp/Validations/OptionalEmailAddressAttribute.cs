using System.ComponentModel.DataAnnotations;

namespace DeKayaServer.BlazorApp.Validations;

public sealed class OptionalEmailAddressAttribute : ValidationAttribute
{
    private readonly EmailAddressAttribute emailAddressAttribute = new();

    public override bool IsValid( object? value )
    {
        if ( value is null )
        {
            return true;
        }

        var email = value.ToString();

        if ( string.IsNullOrWhiteSpace( email ) )
        {
            return true;
        }

        return emailAddressAttribute.IsValid( email );
    }
}
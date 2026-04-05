namespace DeKayaServer.Domain.CustomerBalance.ValueObjects;

public sealed record LastPaymentType( string Value )
{
    public static LastPaymentType Cash => new( "Nakit" );
    public static LastPaymentType CreditCard => new( "Kredi Kartı" );
    public static LastPaymentType BankTransfer => new( "Banka Transferi" );
}

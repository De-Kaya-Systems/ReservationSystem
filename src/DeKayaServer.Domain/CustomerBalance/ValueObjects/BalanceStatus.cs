namespace DeKayaServer.Domain.CustomerBalance.ValueObjects;

public sealed record BalanceStatus( string Value )
{
    public static BalanceStatus Pending => new( "Ödeme yapılmamış" );
    public static BalanceStatus PartiallyPaid => new( "Kısmen ödenmiş" );
    public static BalanceStatus Paid => new( "Ödeme yapılmış" );
}

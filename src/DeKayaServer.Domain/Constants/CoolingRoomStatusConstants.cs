namespace DeKayaServer.Domain.Constants;

public static class CoolingRoomStatusConstants
{
    public const string Available = "Uygun";
    public const string Faulty = "Arıza/Bakım";

    // Legacy: artık yeni iş kurallarında kullanılmayacak
    // EN : Legacy: will no longer be used in new business rules
    public const string Reserved = "Rezerve";
    public const string Booked = "Booked";
}
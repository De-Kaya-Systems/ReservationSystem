namespace DeKayaServer.Application.Services;

public interface IUserContext
{
    Guid GetUserId();
    string GetFullName();
}

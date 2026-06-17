namespace Kron.Counting.Application.Interfaces;

public interface IFirebaseNotificationService
{
    Task<string> SendAsync(
        string token,
        string title,
        string body);
}
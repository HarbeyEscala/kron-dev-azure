using FirebaseAdmin.Messaging;

namespace Kron.Counting.Infrastructure.Notifications;

public sealed class FirebaseNotificationService
{
    public async Task<string> SendAsync(
        string token,
        string title,
        string body)
    {
        var message = new Message
        {
            Token = token,

            Notification = new Notification
            {
                Title = title,
                Body = body
            }
        };

        return await FirebaseMessaging
            .DefaultInstance
            .SendAsync(message);
    }
}
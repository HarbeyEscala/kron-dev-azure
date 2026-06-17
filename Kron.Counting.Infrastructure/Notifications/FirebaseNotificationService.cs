using FirebaseAdmin.Messaging;
using Kron.Counting.Application.Interfaces;
using Kron.Counting.Application.Interfaces;

namespace Kron.Counting.Infrastructure.Notifications;

public sealed class FirebaseNotificationService
    : IFirebaseNotificationService
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
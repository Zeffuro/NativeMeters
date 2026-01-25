using Dalamud.Interface.ImGuiNotification;
using Dalamud.Plugin.Services;
using NativeMeters.Services;

namespace NativeMeters.Extensions;

public static class NotificationExtensions
{
    extension(INotificationManager manager)
    {
        public void Success(string message, bool log = true)
            => manager.Dispatch(message, NotificationType.Success, log);

        public void Error(string message, bool log = true)
            => manager.Dispatch(message, NotificationType.Error, log);

        public void Warning(string message, bool log = true)
            => manager.Dispatch(message, NotificationType.Warning, log);

        private void Dispatch(string message, NotificationType type, bool log)
        {
            manager.AddNotification(new Notification
            {
                Content = message,
                Type = type,
                Title = "NativeMeters"
            });

            if (log)
            {
                switch (type)
                {
                    case NotificationType.Success:
                    case NotificationType.Info:
                    case NotificationType.None:
                        Service.Logger.Info(message);
                        break;
                    case NotificationType.Warning:
                        Service.Logger.Warning(message);
                        break;
                    case NotificationType.Error:
                        Service.Logger.Error(message);
                        break;
                }
            }
        }
    }
}

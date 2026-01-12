using NativeMeters.Services;

namespace NativeMeters.Extensions;

public static class LoggerExtensions
{
    extension(object logger)
    {
        public void DebugOnly(string message)
        {
            if (System.Config?.General?.DebugEnabled == true)
            {
                Service.Logger.Debug(message);
            }
        }

        public void DebugOnly(string message, params object[] args)
        {
            if (System.Config?.General?.DebugEnabled == true)
            {
                Service.Logger.Debug(message, args);
            }
        }
    }
}
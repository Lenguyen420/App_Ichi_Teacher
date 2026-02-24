using kido_teacher_app.Config;
using System;
using System.Net.NetworkInformation;

namespace kido_teacher_app.Shared.Network
{
    public static class OfflineState
    {
        private static readonly object LockObj = new object();
        private static DateTime _lastCheckUtc = DateTime.MinValue;
        private static bool _lastOffline = false;

        public static bool IsOffline()
        {
            lock (LockObj)
            {
                var now = DateTime.UtcNow;
                if ((now - _lastCheckUtc).TotalSeconds < 5)
                    return _lastOffline;

                _lastCheckUtc = now;
                _lastOffline = !HasInternet();
                return _lastOffline;
            }
        }

        private static bool HasInternet()
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
                return false;

            try
            {
                var baseUrl = AppConfig.ApiBaseUrl;
                if (string.IsNullOrWhiteSpace(baseUrl))
                    return false;

                if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri))
                    return false;

                var host = uri.Host;
                if (string.IsNullOrWhiteSpace(host))
                    return false;

                using var ping = new Ping();
                var reply = ping.Send(host, 500);
                return reply != null && reply.Status == IPStatus.Success;
            }
            catch
            {
                return false;
            }
        }
    }
}

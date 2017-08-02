using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitHole.Factories
{
    public class ClientFactory
    {
        private static string _defaultHostName = "localhost";
        private static string _defaulVirtualHost = "/";
        private static string _defaultUserName = "guest";
        private static string _defaultPassword = "guest";

        public static IClient Create() {
            return Create(null);
        }

        public static IClient Create(string name, bool automaticRecoveryEnabled = true, bool topologyRecoveryEnabled = true, int recoveryRetryInterval = 10)
        {
            var connection = new Connection()
                                    .WithHostName(_defaultHostName)
                                    .WithVirtualHost(_defaulVirtualHost)
                                    .WithUserName(_defaultUserName)
                                    .WithPassword(_defaultPassword)
                                    .WithAutomaticRecovery(automaticRecoveryEnabled)
                                    .WithTopologyRecovery(topologyRecoveryEnabled)
                                    .WithRecoveryRetryInterval(recoveryRetryInterval);

            connection.CreateRabbitConnection(name);

            return new Client()
                .WithConnection(connection);
        }

        public static void WithDefaultConnection(Func<IConnection, IConnection> connection) {
            var c = connection(new Connection());
            _defaultHostName = c.HostName;
            _defaulVirtualHost = c.VirtualHost;
            _defaultUserName = c.UserName;
            _defaultPassword = c.Password;
        }
    }
}

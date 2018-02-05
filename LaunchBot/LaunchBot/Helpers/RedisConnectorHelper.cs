using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaunchBot.Helpers
{
    public class RedisConnectorHelper
    {
        private static Lazy<ConnectionMultiplexer> LazyConnection => new Lazy<ConnectionMultiplexer>(() =>
        {
            try
            {
                return ConnectionMultiplexer.Connect("localhost");
            }
            catch (RedisConnectionException ex)
            {
                throw new ArgumentNullException("Ошибка подключения к базе данных Redis. " + ex.ToString(), ex);
            }
        });

        public static ConnectionMultiplexer Connection => LazyConnection.Value;
    }
}

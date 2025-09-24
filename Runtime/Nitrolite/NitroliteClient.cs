using System;
using System.Threading.Tasks;

namespace Nitrolite
{
    public class NitroliteClient
    {
        private INitroTransport transport;

        public NitroliteClient(INitroTransport transport)
        {
            this.transport = transport;
        }

        public Task<string> LoginAsync() => transport.LoginAsync();

        public Task<decimal> GetBalanceAsync(string address, string asset = "ETH")
            => transport.GetBalanceAsync(address, asset);

        public Task<string> MakeTransactionAsync(object txParams)
            => transport.SendTransactionAsync(txParams);

        public Task SubscribeAsync(string channel, Action<string> onMessage)
            => transport.SubscribeAsync(channel, onMessage);
    }
}

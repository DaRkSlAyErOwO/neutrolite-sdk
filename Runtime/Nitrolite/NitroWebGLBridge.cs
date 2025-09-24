using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine;

namespace Nitrolite
{
    public class NitroWebGLBridge : INitroTransport
    {
        #if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] private static extern string Nitrolite_RequestAccounts();
        [DllImport("__Internal")] private static extern string Nitrolite_SignAndSend(string txJson);
        #endif

        public Task<string> LoginAsync()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return Task.FromResult(Nitrolite_RequestAccounts());
#else
            throw new PlatformNotSupportedException("Only works in WebGL builds.");
#endif
        }

        public Task<decimal> GetBalanceAsync(string address, string asset = "ETH")
        {
            throw new NotImplementedException("Balance check must call JS or HTTP RPC.");
        }

        public Task<string> SendTransactionAsync(object txParams)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            string json = JsonUtility.ToJson(txParams);
            return Task.FromResult(Nitrolite_SignAndSend(json));
#else
            throw new PlatformNotSupportedException("Only works in WebGL builds.");
#endif
        }

        public Task SubscribeAsync(string channel, Action<string> onMessage)
        {
            throw new NotImplementedException();
        }
    }
}

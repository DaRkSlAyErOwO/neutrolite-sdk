using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine;

namespace Nitrolite
{
    /// <summary>
    /// WebGL bridge that calls browser wallet methods via a jslib plugin.
    /// Works only in WebGL builds (not in editor).
    /// </summary>
    public class NitroWebGLBridge : INitroTransport
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern string Nitrolite_RequestAccounts();

        [DllImport("__Internal")]
        private static extern string Nitrolite_SignAndSend(string txJson);
#endif

        public Task<string> LoginAsync()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            try
            {
                string addr = Nitrolite_RequestAccounts();
                return Task.FromResult(addr);
            }
            catch (Exception ex)
            {
                return Task.FromException<string>(ex);
            }
#else
            throw new PlatformNotSupportedException("NitroWebGLBridge.LoginAsync() is only supported in WebGL builds.");
#endif
        }

        public Task<decimal> GetBalanceAsync(string address, string asset = "ETH")
        {
            // For WebGL, we recommend calling the RPC from JS (or have the C# HTTP transport call your RPC).
            throw new NotImplementedException("GetBalanceAsync is not implemented in NitroWebGLBridge. Use NitroHttpTransport or extend the JS bridge to query RPC.");
        }

        public Task<string> SendTransactionAsync(object txParams)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            try
            {
                string json = JsonUtility.ToJson(txParams);
                string result = Nitrolite_SignAndSend(json);
                return Task.FromResult(result);
            }
            catch (Exception ex)
            {
                return Task.FromException<string>(ex);
            }
#else
            throw new PlatformNotSupportedException("NitroWebGLBridge.SendTransactionAsync() is only supported in WebGL builds.");
#endif
        }

        public Task SubscribeAsync(string channel, Action<string> onMessage)
        {
            throw new NotImplementedException("Subscribe is not implemented on the WebGL bridge. You may implement event callbacks via JS if needed.");
        }
    }
}

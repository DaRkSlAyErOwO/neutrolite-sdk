using System;
using System.Numerics;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Nitrolite
{
    /// <summary>
    /// Simple JSON-RPC HTTP transport talking to a Nitrolite/Clearnode RPC endpoint.
    /// Uses UnityWebRequest (works for Editor and standalone).
    /// NOTE: This implementation is intentionally minimal and synchronous-friendly for the example.
    /// </summary>
    public class NitroHttpTransport : INitroTransport
    {
        private readonly string rpcBaseUrl;

        public NitroHttpTransport(string rpcBaseUrl)
        {
            if (string.IsNullOrWhiteSpace(rpcBaseUrl)) throw new ArgumentException("rpcBaseUrl");
            this.rpcBaseUrl = rpcBaseUrl.TrimEnd('/');
        }

        public Task<string> LoginAsync()
        {
            // HTTP-based login requires server-side custody or an out-of-band session.
            throw new InvalidOperationException("Login via HTTP requires server-side signing/custody. Use WebGL bridge for wallet-based login.");
        }

        /// <summary>
        /// Queries eth_getBalance and returns decimal in ETH (wei / 1e18).
        /// </summary>
        public async Task<decimal> GetBalanceAsync(string address, string asset = "ETH")
        {
            if (string.IsNullOrWhiteSpace(address)) throw new ArgumentNullException(nameof(address));

            var rpc = new
            {
                jsonrpc = "2.0",
                method = "eth_getBalance",
                @params = new object[] { address, "latest" },
                id = 1
            };

            string json = JsonUtility.ToJson(rpc);

            using (var req = new UnityWebRequest(rpcBaseUrl, "POST"))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
                req.uploadHandler = new UploadHandlerRaw(bodyRaw);
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "application/json");

#if UNITY_2020_1_OR_NEWER
                var op = req.SendWebRequest();
                while (!op.isDone) await Task.Yield();
#else
                await req.SendWebRequest();
#endif

                if (req.result != UnityWebRequest.Result.Success)
                {
                    throw new Exception($"RPC request failed: {req.error}");
                }

                string resp = req.downloadHandler.text;
                // Extract "result":"0x..." with simple parsing.
                string hex = ExtractRpcResultHex(resp);
                if (string.IsNullOrEmpty(hex)) throw new Exception("Failed to parse RPC response: " + resp);

                BigInteger wei = BigInteger.Parse(hex, System.Globalization.NumberStyles.HexNumber);
                decimal eth = (decimal)wei / (decimal)Math.Pow(10, 18);
                return eth;
            }
        }

        public async Task<string> SendTransactionAsync(object txParams)
        {
            // txParams expected to be a signed raw transaction when calling eth_sendRawTransaction.
            // If the caller provides an object with `raw` field, we handle it; otherwise we try to send eth_sendTransaction.
            if (txParams == null) throw new ArgumentNullException(nameof(txParams));

            string jsonParams = JsonUtility.ToJson(txParams);
            // Determine if it's raw tx (contains "raw") — naive check:
            bool looksSigned = jsonParams.Contains("\"raw\"") || jsonParams.Contains("0x");

            object rpcObj;
            if (looksSigned)
            {
                // Expect txParams like: { "raw": "0x..." } or string "0x..."
                string rawHex = TryExtractRawHex(jsonParams);
                rpcObj = new
                {
                    jsonrpc = "2.0",
                    method = "eth_sendRawTransaction",
                    @params = new object[] { rawHex },
                    id = 1
                };
            }
            else
            {
                // Send as eth_sendTransaction (requires an unlocked account on the node or wallet signing).
                // We forward the object as-is inside an array.
                rpcObj = new
                {
                    jsonrpc = "2.0",
                    method = "eth_sendTransaction",
                    @params = new object[] { txParams },
                    id = 1
                };
            }

            string rpcJson = JsonUtility.ToJson(rpcObj);
            using (var req = new UnityWebRequest(rpcBaseUrl, "POST"))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(rpcJson);
                req.uploadHandler = new UploadHandlerRaw(bodyRaw);
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "application/json");

#if UNITY_2020_1_OR_NEWER
                var op = req.SendWebRequest();
                while (!op.isDone) await Task.Yield();
#else
                await req.SendWebRequest();
#endif

                if (req.result != UnityWebRequest.Result.Success)
                {
                    throw new Exception($"RPC request failed: {req.error}");
                }

                string resp = req.downloadHandler.text;
                // Extract result string (tx hash) quickly.
                string result = ExtractRpcResultString(resp);
                if (string.IsNullOrEmpty(result)) throw new Exception("Failed to parse tx hash from: " + resp);
                return result;
            }
        }

        public Task SubscribeAsync(string channel, Action<string> onMessage)
        {
            // WebSocket/subscribe is not implemented in this example.
            throw new NotImplementedException("SubscribeAsync is not implemented in NitroHttpTransport. Use a WebSocket client implementation for realtime events.");
        }

        // --- Helpers (lightweight parsing) ---
        private static string ExtractRpcResultHex(string rpcResponseJson)
        {
            // find `"result":"0x..."`
            const string key = "\"result\":\"";
            int idx = rpcResponseJson.IndexOf(key, StringComparison.OrdinalIgnoreCase);
            if (idx < 0) return null;
            int start = idx + key.Length;
            int end = rpcResponseJson.IndexOf('"', start);
            if (end < 0) return null;
            string val = rpcResponseJson.Substring(start, end - start);
            if (val.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                return val.Substring(2);
            return val;
        }

        private static string ExtractRpcResultString(string rpcResponseJson)
        {
            // find `"result":"0x..."`
            const string key = "\"result\":\"";
            int idx = rpcResponseJson.IndexOf(key, StringComparison.OrdinalIgnoreCase);
            if (idx < 0) return null;
            int start = idx + key.Length;
            int end = rpcResponseJson.IndexOf('"', start);
            if (end < 0) return null;
            return rpcResponseJson.Substring(start, end - start);
        }

        private static string TryExtractRawHex(string json)
        {
            // Very naive: find 0x... substring
            int idx = json.IndexOf("0x", StringComparison.OrdinalIgnoreCase);
            if (idx < 0) return json.Trim('"');
            int end = idx + 2;
            while (end < json.Length && IsHexChar(json[end])) end++;
            return json.Substring(idx, end - idx);
        }

        private static bool IsHexChar(char c)
        {
            return (c >= '0' && c <= '9')
                || (c >= 'a' && c <= 'f')
                || (c >= 'A' && c <= 'F');
        }
    }
}

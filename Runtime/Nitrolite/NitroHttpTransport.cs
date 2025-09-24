using System;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace Nitrolite
{
    public class NitroHttpTransport : INitroTransport
    {
        private string rpcBaseUrl;

        public NitroHttpTransport(string rpcBaseUrl)
        {
            this.rpcBaseUrl = rpcBaseUrl.TrimEnd('/');
        }

        public Task<string> LoginAsync()
        {
            throw new InvalidOperationException(
                "Login via HTTP requires server-side signing or custodian support.");
        }

        public async Task<decimal> GetBalanceAsync(string address, string asset = "ETH")
        {
            string url = $"{rpcBaseUrl}/balance?address={address}&asset={asset}";
            using UnityWebRequest req = UnityWebRequest.Get(url);
            await req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
                throw new Exception(req.error);

            return decimal.Parse(req.downloadHandler.text);
        }

        public async Task<string> SendTransactionAsync(object txParams)
        {
            string url = $"{rpcBaseUrl}/send";
            string json = UnityEngine.JsonUtility.ToJson(txParams);
            using UnityWebRequest req = new UnityWebRequest(url, "POST");
            req.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            await req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
                throw new Exception(req.error);

            return req.downloadHandler.text;
        }

        public Task SubscribeAsync(string channel, Action<string> onMessage)
        {
            throw new NotImplementedException("WebSocket subscription not implemented.");
        }
    }
}

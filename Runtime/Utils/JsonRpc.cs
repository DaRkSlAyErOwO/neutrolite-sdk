using System;
using System.Collections.Generic;

namespace Nitrolite.Utils
{
    [Serializable]
    public class JsonRpcRequest
    {
        public string jsonrpc = "2.0";
        public string method;
        public object[] @params;
        public string id;
    }

    [Serializable]
    public class JsonRpcResponse<T>
    {
        public string jsonrpc;
        public T result;
        public JsonRpcError error;
        public string id;
    }

    [Serializable]
    public class JsonRpcError
    {
        public int code;
        public string message;
    }
}

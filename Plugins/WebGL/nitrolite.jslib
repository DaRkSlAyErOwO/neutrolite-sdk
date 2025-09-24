mergeInto(LibraryManager.library, {
  Nitrolite_RequestAccounts: function () {
    return Asyncify.handleAsync(async () => {
      if (!window.ethereum) {
        alert("No Ethereum provider found. Install MetaMask.");
        return allocateUTF8("");
      }
      const accounts = await window.ethereum.request({ method: "eth_requestAccounts" });
      return allocateUTF8(accounts[0]);
    });
  },

  Nitrolite_SignAndSend: function (txJsonPtr) {
    return Asyncify.handleAsync(async () => {
      if (!window.ethereum) {
        alert("No Ethereum provider found. Install MetaMask.");
        return allocateUTF8("");
      }
      const tx = JSON.parse(UTF8ToString(txJsonPtr));
      const hash = await window.ethereum.request({
        method: "eth_sendTransaction",
        params: [tx],
      });
      return allocateUTF8(hash);
    });
  }
});

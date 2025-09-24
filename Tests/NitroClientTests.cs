using NUnit.Framework;
using System.Threading.Tasks;
using Nitrolite;

public class NitroClientTests
{
    [Test]
    public async Task DummyTest()
    {
        var client = new NitroliteClient(new FakeTransport());
        string login = await client.LoginAsync();
        Assert.AreEqual("0x123", login);
    }

    private class FakeTransport : INitroTransport
    {
        public Task<string> LoginAsync() => Task.FromResult("0x123");
        public Task<decimal> GetBalanceAsync(string a, string b="ETH") => Task.FromResult(1.23m);
        public Task<string> SendTransactionAsync(object t) => Task.FromResult("0xtxhash");
        public Task SubscribeAsync(string c, System.Action<string> o) => Task.CompletedTask;
    }
}

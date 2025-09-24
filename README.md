# Nitrolite Unity

A Unity Package for login, balance check and transactions using [Nitrolite](https://erc7824.org/) & [Clearnode](https://github.com/erc7824/clearnode).

## Installation

In Unity:

- Open `Window → Package Manager`
- Click `+` → `Add package from git URL`
- Paste: `https://github.com/yourorg/nitrolite-unity.git#v0.1.0`

## Usage

```csharp
using Nitrolite;

async void Example()
{
    var client = new NitroliteClient(new NitroWebGLBridge());
    string address = await client.LoginAsync();
    decimal balance = await client.GetBalanceAsync(address);
    Debug.Log($"User: {address}, Balance: {balance}");
}

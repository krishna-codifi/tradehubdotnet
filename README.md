TradeHub Console Demo

This repository contains a C# Console Application that demonstrates end-to-end usage of the TradeHub .NET DLL (In.Codifi.Tradehub).

The demo covers:

Session creation

Profile / funds / positions / holdings APIs

Contract master download

Instrument lookup (cash & F&O)

Order lifecycle (place / modify / cancel)

Orderbook & tradebook

GTT APIs

WebSocket (market data + depth data + keepalive)

⚠️ Important: This demo performs REAL TRADES if enabled. Use only with test credentials or extreme caution.

1. Prerequisites

.NET SDK 10.0+

Windows / Linux / macOS

Active AliceBlue / TradeHub API credentials

2. Project Structure

TradeHubConsole/
├── Program.cs          # Demo console app
├── TradeHub.dll        # Single-file TradeHub library
├── TradeHubConsole.csproj
└── README.md

3. Referencing the TradeHub DLL

Add the DLL reference to your console project:

dotnet add reference path/to/TradeHub.dll

Or in TradeHubConsole.csproj:

<ItemGroup>
  <Reference Include="TradeHub">
    <HintPath>..\\libs\\TradeHub.dll</HintPath>
  </Reference>
</ItemGroup>

4. Configuration

Update credentials in Program.cs:

static string USER_ID = "USER_ID";
static string AUTH_CODE = "YOUR_AUTH_CODE";
static string SECRET_KEY = "YOUR_SECRET_KEY";

🔐 Recommendation: Move secrets to environment variables for production use.

5. Demo Flow Overview

5.1 Create TradeHub Client

var trade = new TradeHub(USER_ID, AUTH_CODE, SECRET_KEY, null);

5.2 Get Session ID

var sessionResp = trade.GetSessionID("", "");
string sessionId = ExtractSessionId(sessionResp);
trade.SetSessionID(sessionId);

The helper safely extracts userSession or accessToken from API response.

5.3 Profile / Funds / Positions / Holdings

trade.InitGet("getProfile", null, "");
trade.InitGet("getFunds", null, "");
trade.InitGet("getPositions", null, "");
trade.GetHoldings("CNC");

5.4 Contract Master Download

trade.GetContractMaster("NSE");

Downloads the latest CSV contract master file.

5.5 Instrument APIs

trade.GetInstrument("NSE", "YESBANK", "");
trade.GetInstrument("NSE", "", "14366");
trade.GetInstrumentForFNO("NFO", "NIFTY", "2026-06-25", false, "30000", false);

5.6 Order APIs (⚠ LIVE TRADES)

trade.PlaceOrder(...);
trade.ModifyOrder(...);
trade.CancelOrder(...);

⚠️ These APIs place and modify real market orders.

5.7 Orderbook & Tradebook

trade.InitGet("getOrderbook", null, "");
trade.InitGet("getTradebook", null, "");

5.8 GTT APIs

trade.GTT_placeOrder(...);

6. WebSocket Demo

6.1 Create ClientWS

using var ws = new ClientWS(sessionId);

6.2 Invalidate & Create WS Session

await ws.InvalidateWebSocketSessionAsync(USER_ID);
await ws.CreateWebSocketSessionAsync(USER_ID);

6.3 Connect WebSocket

var dispatcher = await ws.ConnectWSAsync(sessionId, USER_ID);
var queue = dispatcher.Ch;

6.4 Read Incoming Messages

Task.Run(() =>
{
    foreach (var msg in queue.GetConsumingEnumerable())
        Console.WriteLine(">>> WS IN: " + msg);
});

6.5 KeepAlive & Subscriptions

ws.KeepAlive(ClientWS.PING_INTERVAL, true);
await ws.SubscribeMarketDataAsync("NSE|26000#NSE|26009");
await ws.UnsubscribeMarketDataAsync("NSE|26000#NSE|26009");
await ws.SubscribeDepthDataAsync("NSE|26000#NSE|26009");
await ws.UnsubscribeDepthDataAsync("NSE|26000#NSE|26009");

6.6 Close WebSocket

ws.StopKeepAlive();
await ws.CloseAsync();

7. Running the Demo

dotnet restore
dotnet run

8. Safety Notes

❗ Do NOT run order APIs on production accounts without validation

❗ Always test in non-market hours first

❗ Handle API failures and retries in real applications

9. Features Covered

✔ REST APIs
✔ CSV contract parsing
✔ Instrument resolution
✔ Order lifecycle
✔ GTT orders
✔ WebSocket login
✔ Market & depth subscription
✔ Heartbeat / keepalive
✔ Thread-safe message handling

10. Support

For enhancements:

Auto-reconnect WebSocket

Typed DTO models

NuGet packaging

Logging & metrics

Async-first REST layer

Feel free to extend this demo as needed.

Happy Trading 🚀


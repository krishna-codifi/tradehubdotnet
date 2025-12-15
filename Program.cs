using In.Codifi.Tradehub;
using System;
using System.Text.Json;
using System.Threading.Tasks;

class Program
{
    static string USER_ID = "";
    static string AUTH_CODE = "";
    static string SECRET_KEY = "";

    static async Task Main(string[] args)
    {
        try
        {
            Console.WriteLine("=== TradeHub DLL DEMO ===");

            // ---------------------------------------
            // 1. Create TradeHub
            // ---------------------------------------
            var trade = new TradeHub(USER_ID, AUTH_CODE, SECRET_KEY, null);

            // ---------------------------------------
            // 2. Get Session
            // ---------------------------------------
            Console.WriteLine("\n1) Getting session...");
            var sessionResp = trade.GetSessionID("", "");
            PrintDict(sessionResp);

            string sessionId = ExtractSessionId(sessionResp);
            Console.WriteLine("SESSION ID = " + sessionId);
            trade.SetSessionID(sessionId);

            // ---------------------------------------
            // 3. Profile / Funds / Positions / Holdings
            // ---------------------------------------
            Console.WriteLine("\n2) Get Profile");
            PrintDict(trade.InitGet("getProfile", null, ""));

            Console.WriteLine("\n3) Get Funds");
            PrintDict(trade.InitGet("getFunds", null, ""));

            Console.WriteLine("\n4) Get Positions");
            PrintDict(trade.InitGet("getPositions", null, ""));

            Console.WriteLine("\n5) Get Holdings");
            PrintDict(trade.GetHoldings("CNC"));

            // ---------------------------------------
            // 4. Contract Master
            // ---------------------------------------
            Console.WriteLine("\n6) Get Contract Master");
            PrintDict(trade.GetContractMaster("NSE"));

            // ---------------------------------------
            // 5. Instrument APIs
            // ---------------------------------------
            Console.WriteLine("\n7) Get Instrument (NSE YESBANK)");
            var inst = trade.GetInstrument("NSE", "YESBANK", "");
            Console.WriteLine(inst);

            Console.WriteLine("\n8) Get Instrument by Token");
            Console.WriteLine(trade.GetInstrument("NSE", "", "14366"));

            Console.WriteLine("\n9) Get FNO Instrument");
            Console.WriteLine(trade.GetInstrumentForFNO(
                "NFO", "NIFTY", "2026-06-25", false, "30000", false));

            // ---------------------------------------
            // 6. Order APIs (REAL TRADES)
            // ---------------------------------------
            
            Console.WriteLine("\n10) Place Order");
            PrintDict(trade.PlaceOrder(
                "14366", "NSE", "Buy", "1",
                "REGULAR", "INTRADAY", "MARKET",
                "0", "0", "0", "0", "DAY"));

            Console.WriteLine("\n11) Modify Order");
            PrintDict(trade.ModifyOrder(
                "25121500081428", "100", "0", "", "",
                "1", "LIMIT", "", "DAY", "", "", TradeHub.GetDeviceID()));

            Console.WriteLine("\n12) Cancel Order");
            PrintDict(trade.CancelOrder("25121500080990"));
            

            // ---------------------------------------
            // 7. Order Books
            // ---------------------------------------
            Console.WriteLine("\n13) Order Book");
            PrintDict(trade.InitGet("getOrderbook", null, ""));

            Console.WriteLine("\n14) Trade Book");
            PrintDict(trade.InitGet("getTradebook", null, ""));

            // ---------------------------------------
            // 8. GTT APIs
            // ---------------------------------------
            
            Console.WriteLine("\n15) GTT Place");
            PrintDict(trade.GTT_placeOrder(
                "BUY", "1", "REGULAR", "INTRADAY", "LIMIT",
                "100", "10", "DAY", inst, "", null, ""));
            

            // ---------------------------------------
            // 9. WebSocket APIs
            // ---------------------------------------
            Console.WriteLine("\n16) WebSocket Flow");

            using var ws = new ClientWS(sessionId);

            Console.WriteLine("Invalidate WS Session");
            Console.WriteLine(await ws.InvalidateWebSocketSessionAsync(USER_ID));

            Console.WriteLine("Create WS Session");
            Console.WriteLine(await ws.CreateWebSocketSessionAsync(USER_ID));

            var dispatcher = await ws.ConnectWSAsync(sessionId, USER_ID);
            var queue = dispatcher.Ch;

            // Reader thread
            _ = Task.Run(() =>
            {
                foreach (var msg in queue.GetConsumingEnumerable())
                    Console.WriteLine(">>> WS IN: " + msg);
            });

            ws.KeepAlive(ClientWS.PING_INTERVAL, true);

            Console.WriteLine("Subscribe Market Data");
            await ws.SubscribeMarketDataAsync("NSE|26000#NSE|26009");

            await Task.Delay(10000);

            Console.WriteLine("Unsubscribe Market Data");
            await ws.UnsubscribeMarketDataAsync("NSE|26000#NSE|26009");

            await Task.Delay(10000);

            Console.WriteLine("Subscribe Depth Data");
            await ws.SubscribeDepthDataAsync("NSE|26000#NSE|26009");

            await Task.Delay(10000);

            Console.WriteLine("Unsubscribe Depth Data");
            await ws.UnsubscribeDepthDataAsync("NSE|26000#NSE|26009");

            await Task.Delay(5000);

            ws.StopKeepAlive();
            await ws.CloseAsync();

            Console.WriteLine("\n=== DEMO COMPLETED SUCCESSFULLY ===");
        }
        catch (Exception ex)
        {
            Console.WriteLine("ERROR:");
            Console.WriteLine(ex);
        }
    }

    // ================= HELPERS =================

    static void PrintDict(Dictionary<string, object> dict)
    {
        if (dict == null) return;
        Console.WriteLine(JsonSerializer.Serialize(dict, new JsonSerializerOptions
        {
            WriteIndented = true
        }));
    }

    static string ExtractSessionId(Dictionary<string, object> resp)
    {
        if (resp == null) return "";

        if (resp.TryGetValue("userSession", out var us))
            return us.ToString();

        if (resp.TryGetValue("result", out var r) && r is JsonElement je)
        {
            foreach (var item in je.EnumerateArray())
                if (item.TryGetProperty("accessToken", out var at))
                    return at.GetString();
        }

        throw new Exception("SessionId not found");
    }
}

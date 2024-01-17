using ProtoBuf;
using System.Net.WebSockets;
using System.Text;

namespace YahooFinanceWebSocket
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            using var client = new ClientWebSocket();
            var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(1200));

            try
            {
                await client.ConnectAsync(new Uri("wss://streamer.finance.yahoo.com/"), cts.Token);
                var message = "{\"subscribe\":[\"INVE-B.ST\", \"^OMX\", \"SBB-B.ST\"]}";
                var byteToSend = new ArraySegment<byte>(Encoding.UTF8.GetBytes(message));

                if (client.State == WebSocketState.Open)
                {
                    Console.WriteLine("Connection successful.");
                    await client.SendAsync(byteToSend, WebSocketMessageType.Text, true, cts.Token);
                    var responseBuffer = new byte[1024];
                    var offset = 0;
                    var packet = 1024;
                    while (true)
                    {
                        var byteReceived = new ArraySegment<byte>(responseBuffer, offset, packet);
                        var response = await client.ReceiveAsync(byteReceived, cts.Token);
                        var responseMessage = Encoding.UTF8.GetString(responseBuffer, offset, response.Count);

                        var x = Convert.FromBase64String(responseMessage);
                        var y = Serializer.Deserialize<PricingData>((ReadOnlySpan<byte>)x);

                        Console.WriteLine($"{y.Id}: {y.Price}, at {DateTimeOffset.FromUnixTimeMilliseconds(y.Time).TimeOfDay}");
                    }
                }
            }
            catch (WebSocketException e) { Console.WriteLine(e.Message); }
        }
    }
}
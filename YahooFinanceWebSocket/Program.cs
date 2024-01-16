using ProtoBuf;
using System.Net.WebSockets;
using System.Text;

namespace YahooFinanceWebSocket
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            using (ClientWebSocket client = new ClientWebSocket())
            {
                Uri serviceUri = new Uri("wss://streamer.finance.yahoo.com/");
                var cancellationToken = new CancellationTokenSource();
                cancellationToken.CancelAfter(TimeSpan.FromSeconds(1200));

                try
                {
                    await client.ConnectAsync(serviceUri, cancellationToken.Token);
                    string message = "{\"subscribe\":[\"ETH-USD\", \"BTC-USD\", \"YM=F\", \"ES=F\", \"QM=F\", \"RTY=F\", \"MNQ=F\", \"NQ=F, \"\"^GSPC\", \"INVE-B.ST\", \"^OMX\", \"SBB-B.ST\"]}";
                    ArraySegment<byte> byteToSend = new ArraySegment<byte>(Encoding.UTF8.GetBytes(message));

                    if (client.State == WebSocketState.Open)
                    {
                        Console.WriteLine("Connection successful.");
                        await client.SendAsync(byteToSend, WebSocketMessageType.Text, true, cancellationToken.Token);
                        var responseBuffer = new byte[1024];
                        var offset = 0;
                        var packet = 1024;
                        while (true)
                        {
                            ArraySegment<byte> byteReceived = new ArraySegment<byte>(responseBuffer, offset, packet);
                            WebSocketReceiveResult response = await client.ReceiveAsync(byteReceived, cancellationToken.Token);
                            var responseMessage = Encoding.UTF8.GetString(responseBuffer, offset, response.Count);

                            var x = Convert.FromBase64String(responseMessage);
                            var y = Serializer.Deserialize<PricingData>((ReadOnlySpan<byte>)x);

                            Console.WriteLine($"{y.Id}: {y.Price}, at {DateTimeOffset.FromUnixTimeMilliseconds(y.Time).DateTime}");
                        }
                    }
                }
                catch (WebSocketException e) { Console.WriteLine(e.Message); }

            }
            Console.ReadLine();
        }
    }
}

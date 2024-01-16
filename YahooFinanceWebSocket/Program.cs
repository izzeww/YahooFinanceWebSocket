using System.Net.WebSockets;
using System.Text;

namespace YahooFinanceWebSocket
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            using (ClientWebSocket client = new ClientWebSocket())
            {
                Uri serviceUri = new Uri("wss://streamer.finance.yahoo.com/");
                var cancellationToken = new CancellationTokenSource();
                cancellationToken.CancelAfter(TimeSpan.FromSeconds(120));

                try
                {
                    await client.ConnectAsync(serviceUri, cancellationToken.Token);
                    string message = "{\"subscribe\":[\"ETH-USD\", \"BTC-USD\", \"YM=F\", \"ES=F\", \"QM=F\", \"RTY=F\", \"MNQ=F\", \"NQ=F\"]}";
                    ArraySegment<byte> byteToSend = new ArraySegment<byte>(Encoding.UTF8.GetBytes(message));

                    if (client.State == WebSocketState.Open)
                    {
                        await client.SendAsync(byteToSend, WebSocketMessageType.Text, true, cancellationToken.Token);
                        Console.WriteLine("test");
                        var responseBuffer = new byte[1024];
                        var offset = 0;
                        var packet = 1024;
                        while (true)
                        {
                            ArraySegment<byte> byteReceived = new ArraySegment<byte>(responseBuffer, offset, packet);
                            WebSocketReceiveResult response = await client.ReceiveAsync(byteReceived, cancellationToken.Token);
                            var responseMessage = Encoding.UTF8.GetString(responseBuffer, offset, response.Count);
                            Console.WriteLine(responseMessage); // this prints the response in base64
                                                                // protobuf-encoded form to the console, works fine
                        }
                    }
                }
                catch (WebSocketException e) { Console.WriteLine(e.Message); }

            }
            Console.ReadLine();
        }

    }
}

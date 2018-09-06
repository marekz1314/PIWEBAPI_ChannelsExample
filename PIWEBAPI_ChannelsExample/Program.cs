using System;
using System.Text;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Threading;

namespace PIWEBAPI_ChannelsExample
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Press any key to close.");
            CancellationTokenSource cancellationSource = new CancellationTokenSource();
            Task runTask = RunClient(cancellationSource.Token);
            Console.ReadKey();
            cancellationSource.Cancel();
            runTask.Wait();
        }
        public static async Task RunClient(CancellationToken cancellationToken)
        {
            Uri uri = new Uri("wss://192.168.2.31/piwebapi/streams/F1DPXo6Tq7dbDESaFakUUXB--wRQcAAAUElTUlZUXEFGU0RLV1NfU0lOVVNPSUQ/channel");

            WebSocketReceiveResult receiveResult;
            byte[] receiveBuffer = new byte[65536];
            ArraySegment<byte> receiveSegment = new ArraySegment<byte>(receiveBuffer);

            using (ClientWebSocket webSocket = new ClientWebSocket())
            {
                try
                {
                    await webSocket.ConnectAsync(uri, CancellationToken.None);
                }
                catch (WebSocketException)
                {
                    Console.WriteLine("Could not connect to server.");
                    return;
                }

                while (true)
                {
                    try
                    {
                        receiveResult = await webSocket.ReceiveAsync(receiveSegment, cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }

                    if (receiveResult.MessageType != WebSocketMessageType.Text)
                    {
                        await webSocket.CloseAsync(
                            WebSocketCloseStatus.InvalidMessageType,
                            "Message type is not text.",
                            CancellationToken.None);
                        return;
                    }
                    else if (receiveResult.Count > receiveBuffer.Length)
                    {
                        await webSocket.CloseAsync(
                            WebSocketCloseStatus.InvalidPayloadData,
                            "Message is too long.",
                            CancellationToken.None);
                        return;
                    }

                    string message = Encoding.UTF8.GetString(receiveBuffer, 0, receiveResult.Count);

                    Console.WriteLine(message);
                }

                await webSocket.CloseAsync(
                    WebSocketCloseStatus.NormalClosure,
                    "Closing connection.",
                    CancellationToken.None);
            }
        }
    }
}

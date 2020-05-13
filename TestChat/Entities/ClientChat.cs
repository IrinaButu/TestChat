using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestChat.Entities
{
    public class ClientChat
    {
        public static async Task Connect(ClientWebSocket socket, string Connection)
        {
            await socket.ConnectAsync(new Uri(Connection), CancellationToken.None);
        }

        public static async Task Disconnect(ClientWebSocket socket, string message)
        {
            await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, message, CancellationToken.None);
        }
        public static async Task Send(ClientWebSocket socket, ChatMessage message)
        {
            var data = JsonConvert.SerializeObject(message);
            var encoded = Encoding.UTF8.GetBytes(data);
            var buffer = new ArraySegment<Byte>(encoded, 0, encoded.Length);

            await socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
        }

        public static async Task Receive(ClientWebSocket socket)
        {
            var buffer = new ArraySegment<byte>(new byte[2048]);
            //do
            //{
            WebSocketReceiveResult result;
            using (var ms = new MemoryStream())
            {
                do
                {
                    result = await socket.ReceiveAsync(buffer, CancellationToken.None);
                    ms.Write(buffer.Array, buffer.Offset, result.Count);
                } while (!result.EndOfMessage);

                if (result.MessageType != WebSocketMessageType.Close)
                {

                    ms.Seek(0, SeekOrigin.Begin);

                    var chatMessage = new ChatMessage() { User = "", Message = "" };
                    using (var reader = new StreamReader(ms, Encoding.UTF8))
                    {
                        chatMessage = JsonConvert.DeserializeObject<ChatMessage>(await reader.ReadToEndAsync());
                    }

                    Console.WriteLine(string.IsNullOrEmpty(chatMessage.User) ? chatMessage.Message : chatMessage.User + ":" + chatMessage.Message);
                }
            }
            /* } while (true);*/
        }
    }
}

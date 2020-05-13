using Fleck;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestChat.Entities
{
    public class ServerChat
    {
        private static List<IWebSocketConnection> allSockets = new List<IWebSocketConnection>();

        public static void StartChat(int portNo)
        {
            //FleckLog.Level = LogLevel.Debug;
            if (allSockets.Count > 0)
                return;

            var server = new WebSocketServer("ws://0.0.0.0:" + portNo);
            server.Start(socket =>
            {
                socket.OnOpen = () =>
                {
                    //Console.WriteLine("Open!");
                    //if(allSockets.FindIndex(s => s.ConnectionInfo.Host == socket.ConnectionInfo.Host) < 0)
                    allSockets.Add(socket);
                };
                socket.OnClose = () =>
                {
                    Console.WriteLine("Close!");
                    allSockets.Remove(socket);
                };
                socket.OnMessage = message =>
                {
                    //var chatMessage = JsonConvert.DeserializeObject<ChatMessage>(message);
                    //allSockets.ToList().ForEach(s => s.Send(chatMessage.User + ": " + chatMessage.Message));

                    //Console.WriteLine(message);

                    allSockets.ToList().ForEach(s => s.Send(message));
                };
            });
        }

    }
}

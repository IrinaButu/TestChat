using System;
using System.Net.WebSockets;
using System.Threading.Tasks;
using TestChat.Entities;

namespace TestChat
{
    class Program
    {
        private static readonly string Connection = "ws://localhost";

        static async Task Main(string[] args)
        {
            int portNo = 0;
            string userName = "";

            if (args.Length > 0)
            {
                Int32.TryParse(args[0], out portNo);
                userName = args[1];
                if (portNo == 0 || string.IsNullOrEmpty(userName))
                {
                    Console.WriteLine("Invalid command! The format is: TestChat [portNo] [userName]!");
                    Console.ReadLine();
                    return;
                }
                ServerChat.StartChat(portNo);
            }
            else
            {
                Console.WriteLine("Invalid command! The format is: TestChat [portNo] [userName]!");
                Console.ReadLine();
                return;
            }

            using (var socket = new ClientWebSocket())
            {
                try
                {
                    await ClientChat.Connect(socket, Connection + ":" + portNo);

                    var input = userName + " connected!";
                    ChatMessage chatMessage = new ChatMessage() { User = "", Message = input };

                    await ClientChat.Send(socket, chatMessage);
                    Console.WriteLine("Press exit for exit!");

                    chatMessage.User = userName;

                    bool exit = false;
                    do
                    {
                        await ClientChat.Receive(socket);

                        input = Console.ReadLine();

                        switch (input)
                        {
                            case "leaveChat":
                                chatMessage.Message = userName + " disconnected!";
                                await ClientChat.Send(socket, chatMessage);
                                await ClientChat.Disconnect(socket, chatMessage.Message);
                                exit = true;
                                break;
                            case "":
                            case "exit":
                                break;
                            default:
                                chatMessage.Message = input;
                                await ClientChat.Send(socket, chatMessage);
                                break;
                        }

                    } while (!exit);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR - {ex.Message}");
                }
            }
        }
    }
}
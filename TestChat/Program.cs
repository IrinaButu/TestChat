using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.WebSockets;
using System.Threading.Tasks;
using TestChat.Entities;
using TestChat.Services;

namespace TestChat
{
    class Program
    {
        private static readonly string Connection = "ws://localhost";

        static async Task Main(string[] args)
        {
            //setup our DI
            var serviceProvider = new ServiceCollection()
                .AddSingleton<ClientService, ClientService>()
                .BuildServiceProvider();

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

            var clientService = serviceProvider.GetService<ClientService>();

            await StartChat(clientService, userName, portNo);

        }

        private static async Task StartChat(ClientService clientService, string userName, int portNo)
        {
            using (var socket = new ClientWebSocket())
            {
                try
                {
                    await clientService.Connect(socket, Connection + ":" + portNo);

                    var input = userName + " connected!";
                    ChatMessage chatMessage = new ChatMessage() { User = "", Message = input };

                    await clientService.Send(socket, chatMessage);
                    Console.WriteLine("Type leaveChat for exit!");

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
                                await clientService.Send(socket, chatMessage);
                                await clientService.Disconnect(socket, chatMessage.Message);
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
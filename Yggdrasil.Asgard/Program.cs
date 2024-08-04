using System.Net;
using Yggdrasil.Core.Network.Sockets;

namespace Yggdrasil.Asgard;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Starting server...");
        var server = new Server(new IPEndPoint(IPAddress.Any, 5000), true);
        Console.WriteLine("Server initialized. Press Enter to start listening for connections.");
        Console.ReadLine();
        
        Console.WriteLine("Server is now listening for connections.");
        await server.StartAsync();
    }
}
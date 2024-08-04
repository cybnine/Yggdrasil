using System.Net;

namespace Yggdrasil.Midgard;

class Program
{
    static async Task Main(string[] args)
    {
        const int maxRetries = 5;
        const int retryDelayMs = 2000;

        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                Console.WriteLine($"Attempting to connect to server (attempt {i + 1} of {maxRetries})...");
                var client = new Client(new IPEndPoint(IPAddress.Loopback, 5000));
                Console.WriteLine("Connected successfully. Starting client...");
                await client.StartAsync();
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to connect: {ex.Message}");
                if (i < maxRetries - 1)
                {
                    Console.WriteLine($"Retrying in {retryDelayMs / 1000} seconds...");
                    await Task.Delay(retryDelayMs);
                }
                else
                {
                    Console.WriteLine("Max retries reached. Exiting.");
                }
            }
        }
    }
}
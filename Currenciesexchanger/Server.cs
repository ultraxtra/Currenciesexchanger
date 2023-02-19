using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new CurrencyExchangeServer();
            server.Start();

            Console.WriteLine("Server started, press any key to stop...");
            Console.ReadKey();

            server.Stop();

        }
    }
}

public class CurrencyExchangeServer
{
    private TcpListener listener;
    private ConcurrentDictionary<string, TcpClient> clients = new ConcurrentDictionary<string, TcpClient>();
    public void Start()
    {
        listener = new TcpListener(IPAddress.Any, 8080);
        listener.Start();
        Task.Run(async () =>
        {
            while (true)
            {
                TcpClient client = await listener.AcceptTcpClientAsync();
                string id = Guid.NewGuid().ToString();
                clients.TryAdd(id, client);
                Console.WriteLine($"Client{id} connected at {DateTime.Now}");
                Task.Run(() => HandleClientAsync(id, client));
            }
        });
    }

    public void Stop()
    {
        listener.Stop();
        foreach (var client in clients.Values)
        {
            client.Close();
        }
        Console.WriteLine("Server stopped");
    }

    private async Task HandleClientAsync(string id, TcpClient client)
    {
        var stream = client.GetStream();
        while(true)
        {
            byte[] buffer = new byte[1024];
            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            if(bytesRead == 0)
            {
                break;
            }

            string message = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);
            string[] currencies = message.Split(' ');
            if(currencies.Length == 2)
            {
                string from = currencies[0];
                string to = currencies[1];
                double rate = GetExchangeRate(from, to);
                string response = rate.ToString();
                byte[] responseBytes = System.Text.Encoding.UTF8.GetBytes(response);
                await stream.WriteAsync(responseBytes, 0, responseBytes.Length);
                Console.WriteLine($"Exchange rate from {from} to {to} sent to client {id}: {rate}");
            }
            else
            {
                Console.WriteLine($"Invalid message received from client {id}: {message}");
            }
        }

        client.Close();
        TcpClient removedClient;
        clients.TryRemove(id, out removedClient );
        Console.WriteLine($"Client{id} disconnected at {DateTime.Now}");
    }

    private double GetExchangeRate(string from, string to) 
    {
        return 1.2;
    }
}


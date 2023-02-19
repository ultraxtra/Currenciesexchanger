using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                TcpClient client = new TcpClient("localhost", 8080);
                Console.WriteLine("Connected to server.");

                NetworkStream stream = client.GetStream();

                Console.WriteLine("Enter two currency codes (e.g. USD EUR): ");
                string message = Console.ReadLine();

                byte[] request = System.Text.Encoding.ASCII.GetBytes(message);
                stream.Write(request, 0, request.Length);

                byte[] response = new byte[256];
                int bytesRead = stream.Read(response, 0, response.Length);
                string rate = System.Text.Encoding.ASCII.GetString(response, 0, bytesRead);

                Console.WriteLine("Exchange rate: " + rate);
                stream.Close();
                client.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }
    }
}


using System;
using System.Threading;

// Test: TCP Server + Client Linkage Test
class TcpTestProgram
{
    const string HOST = "127.0.0.1";
    const int PORT = 9999;

    // Server Business Logic: Check for Prime Number
    // Return: $ = Not Prime | $$ = Prime (exactly the same as Python)
    static byte[] IsPrime(byte[] msg)
    {
        try
        {
            string str = System.Text.Encoding.UTF8.GetString(msg);
            int n = int.Parse(str);

            if (n < 2)
                return Utils.HALF_EOQ.AsSingleByteArray(); // $

            for (int i = 2; i <= Math.Sqrt(n); i++)
            {
                if (n % i == 0)
                    return Utils.HALF_EOQ.AsSingleByteArray(); // $
            }

            return Utils.Eoq(); // $$
        }
        catch
        {
            return Utils.HALF_EOQ.AsSingleByteArray();
        }
    }

    static void Main()
    {
        Console.WriteLine("=== C# TCP Test Started ===");

        // 1. Create Server
        var quitToken = System.Text.Encoding.UTF8.GetBytes("quit");
        var server = new SimpleTcpServer(HOST, PORT, IsPrime, quitToken);

        // 2. Run the server on a BACKGROUND DAEMON THREAD
        Thread serverThread = new Thread(() =>
        {
            try { server.MainLoop(); }
            catch { }
        })
        { IsBackground = true };

        serverThread.Start();
        Thread.Sleep(200);
        Console.WriteLine("Server started\n");

        // 3. Create Client
        using var client = new SimpleTcpClient(HOST, PORT);

        // 4. Test Numbers
        byte[][] numbers = {
            System.Text.Encoding.UTF8.GetBytes("2"),
            System.Text.Encoding.UTF8.GetBytes("10"),
            System.Text.Encoding.UTF8.GetBytes("17"),
            System.Text.Encoding.UTF8.GetBytes("97"),
            System.Text.Encoding.UTF8.GetBytes("1234567")
        };

        // First Request
        Console.WriteLine("=== First Request ===");
        foreach (var num in numbers)
        {
            byte[]? res = client.Request(num);
            string numStr = System.Text.Encoding.UTF8.GetString(num);
            string resStr = res == null ? "Disconnected" : System.Text.Encoding.UTF8.GetString(res);
            Console.WriteLine($"{numStr}: {resStr}");
        }

        // Send quit to shut down the server
        Console.WriteLine("\n=== Sending quit to shut down server ===");
        client.Request(System.Text.Encoding.UTF8.GetBytes("quit"));
        Thread.Sleep(100); // Give the server time to close

        // Request again after server is closed
        Console.WriteLine("\n=== Request again after server shutdown ===");
        foreach (var num in numbers)
        {
            byte[]? res = client.Request(num);
            string numStr = System.Text.Encoding.UTF8.GetString(num);
            string resStr = res == null ? "No Response (Closed)" : System.Text.Encoding.UTF8.GetString(res);
            Console.WriteLine($"{numStr}: {resStr}");
        }

        // Safely close client & server
        client.Close();
        server.Dispose();

        Console.WriteLine("\n=== Test Completed ===");

        // ====================== Fix: Remove this line and the program will exit automatically ======================
        // Console.ReadLine();
    }
}

// Utility Extension Methods
public static class ByteArrayExtensions
{
    public static byte[] AsSingleByteArray(this byte b)
    {
        return new[] { b };
    }
}
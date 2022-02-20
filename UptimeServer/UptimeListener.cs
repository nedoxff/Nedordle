using System.Net;
using System.Text;
using Serilog;

namespace Nedordle.UptimeServer;

public class UptimeListener
{
    private static async Task Listen(string placeholder)
    {
        var listener = new HttpListener();
        listener.Prefixes.Add("http://localhost:3000/");
        listener.Start();
        Log.Information("Started uptime server");

        var responseString =
            $"<html><center><h1>{placeholder}</h1><h2>Nedordle&trade;, all rights deleted.</h2></center></html>";
        while (true)
        {
            var context = await listener.GetContextAsync();
            var response = context.Response;

            var buffer = Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            var output = response.OutputStream;
            await output.WriteAsync(buffer);
            output.Close();
        }
        // ReSharper disable once FunctionNeverReturns
    }

    public static void Start(string placeholder)
    {
        new Thread(Listen(placeholder).GetAwaiter().GetResult)
        {
            Name = "Uptime Server",
            IsBackground = true
        }.Start();
    }
}
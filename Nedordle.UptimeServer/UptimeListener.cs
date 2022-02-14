using System.Net;
using System.Text;
using Serilog;

namespace Nedordle.UptimeServer;

public class UptimeListener
{
    private static async Task Listen()
    {
        var listener = new HttpListener();
        listener.Prefixes.Add("http://localhost:3000/");
        listener.Start();
        Log.Information("Started uptime server");

        while (true)
        {
            var context = await listener.GetContextAsync();
            var response = context.Response;

            const string responseString =
                "<html><center><h1>i dont know what you expect to see here</h1></center></html>";
            var buffer = Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            var output = response.OutputStream;
            await output.WriteAsync(buffer);
            output.Close();
        }
        // ReSharper disable once FunctionNeverReturns
    }

    public static void Start()
    {
        new Thread(Listen().GetAwaiter().GetResult)
        {
            Name = "Uptime Server",
            IsBackground = true
        }.Start();
    }
}
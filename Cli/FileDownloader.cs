namespace Nedordle.Cli;

public class FileDownloader
{
    public static void DownloadFile(string url, string path)
    {
        var client = new HttpClient();
        var response = client.GetAsync(url).Result;
        if (!response.IsSuccessStatusCode)
            throw new Exception(
                $"Received a non-success status code when sending a GET request to \"{path}\". Status code: {response.StatusCode.ToString()}");
        var fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write);
        response.Content.CopyTo(fs, null, CancellationToken.None);
        fs.Dispose();
        client.Dispose();
    }
}
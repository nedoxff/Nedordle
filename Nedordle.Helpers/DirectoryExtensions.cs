namespace Nedordle.Helpers;

public class DirectoryExtensions
{
    public static void DeleteWithFiles(string path)
    {
        foreach(var file in Directory.GetFiles(path, "*.*", SearchOption.AllDirectories))
            File.Delete(file);
        Directory.Delete(path, true);
    }
}
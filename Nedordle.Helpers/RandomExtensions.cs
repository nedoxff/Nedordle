namespace Nedordle.Helpers;

public static class RandomExtensions
{
    public static Random New() => new(Guid.NewGuid().GetHashCode());

    public static T NextElement<T>(this Random random, IEnumerable<T> enumerable)
    {
        var list = enumerable.ToList();
        return list[random.Next(0, list.Count)];
    }

    private const string Chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    
    public static string NextString(this Random random, int length)
    {
        return new string(Enumerable.Repeat(Chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
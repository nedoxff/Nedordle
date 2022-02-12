namespace Nedordle.Helpers;

public static class RandomExtensions
{
    private const string Chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    public static Random New()
    {
        return new(Guid.NewGuid().GetHashCode());
    }

    public static T NextElement<T>(this Random random, IEnumerable<T> enumerable)
    {
        var list = enumerable.ToList();
        return list[random.Next(0, list.Count)];
    }

    public static string NextString(this Random random, int length)
    {
        return new string(Enumerable.Repeat(Chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
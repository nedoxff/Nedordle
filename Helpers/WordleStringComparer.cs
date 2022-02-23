namespace Nedordle.Helpers;

public class WordleStringComparer
{
    public static (string, string) Compare(string input, string answer)
    {
        var resultFormatted = "";
        var resultLetters = "";
        var answerArr = answer.ToCharArray();
        var inputArr = input.ToCharArray();

        for (var index = 0; index < input.Length; index++)
        {
            var letter = inputArr[index];
            if (Array.IndexOf(answerArr, letter) == -1)
            {
                resultFormatted += $"[w]{letter}[/]";
                resultLetters += 'w';
                inputArr[index] = '.';
            }
            else if (Array.IndexOf(answerArr, letter) != -1 && answerArr[index] != letter)
            {
                resultFormatted += $"[d]{letter}[/]";
                resultLetters += 'd';
                answerArr[Array.IndexOf(answerArr, letter)] = '.';
                inputArr[index] = '.';
            }
            else
            {
                resultFormatted += $"[c]{letter}[/]";
                resultLetters += 'c';
                answerArr[index] = '.';
                inputArr[index] = '.';
            }
        }

        return (resultFormatted, resultLetters);
    }
}
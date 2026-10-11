using System.Text;
using Yitter.IdGenerator;

namespace Blog.Shared.Functional;

public static class RandomGenerator
{
    public static string Guid()  => System.Guid.NewGuid().ToString();
    public static long SnowflakeId() => YitIdHelper.NextId();

    private const string Numbers = "0123456789";
    private const string Alphabet = "0123456789AaBbCcDdEeFfGgHhIiJjKkLlMmNnOoPpQqRrSsTtUuVvWwXxYyZz";
    public static string GenerateNum(int count, bool startWithZero = false)
    {
        if(count == 0) return string.Empty;
        var builder = new StringBuilder();
        if(startWithZero)
        {
            builder.Append(Random.Shared.Next(1,Numbers.Length));
            count--;
        }
        for (int i = 0; i < count; i++)
            builder.Append(Numbers[Random.Shared.Next(0,Numbers.Length)]);
        return builder.ToString();
    }
    
    public static string GenerateByAlphabet(int count)
    {
        if(count == 0) return string.Empty;
        var builder = new StringBuilder();
        for (int i = 0; i < count; i++)
            builder.Append(Alphabet[Random.Shared.Next(0,Alphabet.Length)]);
        return builder.ToString();
    }
}
using System.Security.Cryptography;

namespace ShortLink.Api.Services;

public static class CodeGenerator
{
    private const string Alphabet = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

    public static string Generate(int length = 7)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(length);

        return string.Create(length, Alphabet, static (code, alphabet) =>
        {
            for (var index = 0; index < code.Length; index++)
            {
                code[index] = alphabet[RandomNumberGenerator.GetInt32(alphabet.Length)];
            }
        });
    }

    public static bool IsValid(string code) =>
        code.Length == 7 && code.All(character => Alphabet.Contains(character));
}

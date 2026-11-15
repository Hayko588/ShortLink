using ShortLink.Api.Services;

namespace ShortLink.Tests;

public sealed class CodeGeneratorTests
{
    [Fact]
    public void Generate_ReturnsSevenCharacterBase62Code()
    {
        var code = CodeGenerator.Generate();

        Assert.Equal(7, code.Length);
        Assert.All(code, character => Assert.Contains(character, "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ"));
    }

    [Fact]
    public void IsValid_RejectsWrongLengthAndCharacters()
    {
        Assert.True(CodeGenerator.IsValid("aB09zY1"));
        Assert.False(CodeGenerator.IsValid("short"));
        Assert.False(CodeGenerator.IsValid("abc123!"));
    }
}

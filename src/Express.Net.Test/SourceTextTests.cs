using Express.Net.CodeAnalysis.Text;
using Xunit;

namespace Express.Net.Test;

public class SourceTextTests
{
    [Theory]
    [InlineData(".", 1)]
    [InlineData(".\r\n", 2)]
    [InlineData(".\r\n\r\n", 3)]
    public void IncludesLastLine(string text, int expectedLineCount)
    {
        var sourceText = SourceText.From(text);
        Assert.Equal(expectedLineCount, sourceText.Lines.Length);
    }
}

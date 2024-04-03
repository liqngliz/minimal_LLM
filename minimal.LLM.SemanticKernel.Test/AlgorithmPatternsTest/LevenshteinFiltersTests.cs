using minimal.LLM.SemanticKernel;

namespace LevenshteinFiltersTests;

public class LevenshteinFiltersTests
{
    [Theory]
    [InlineData("I want to add two numbers", "add")]
    [InlineData("I want to subtract two numbers", "subtract")]
    public void Should_contain_target_in_matrix(string phrase, string target)
    {
       var sut = phrase.ToFlatCharacterStringMatrix();
       Assert.Contains(target, sut);
    }

    [Theory]
    [InlineData("I want to add two numbers", "add", "add")]
    [InlineData("I want to suabtract two numbers", "subtract", "suabtract")]
    [InlineData("I want to perform subtraction", "subtract", "subtracti")]
    public void Should_contain_target_find_match(string phrase, string input, string target)
    {
       var matrix = phrase.ToFlatCharacterStringMatrix();
       var sut = matrix.Distinct().ToList().FilterLevenshteinTolerance(input);
       Assert.Contains(target, sut);
    }

    [Theory]
    [InlineData("I want to multiply two numbers 58*39", "add", "add")]
    public void Should_no_match(string phrase, string input, string target)
    {
       var matrix = phrase.ToFlatCharacterStringMatrix();
       var sut = matrix.Distinct().ToList().FilterLevenshteinTolerance(input);
       Assert.DoesNotContain(target, sut);
    }
}

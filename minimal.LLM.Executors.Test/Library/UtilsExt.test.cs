
using UtilsExt;

namespace UtilsExtTest;

[Collection("Sequential")]
public class EnumerableExtTest
{
    [Fact]
    public void Should_give_index()
    {   
        var nums = new int[]{5,4,3,2,1,0};
        foreach (var num in nums.WithIndex<int>()) 
            Assert.Equal(num.item, nums[num.index]);
    }
    
}

[Collection("Sequential")]
public class LevenshteinUtilsExtTest
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
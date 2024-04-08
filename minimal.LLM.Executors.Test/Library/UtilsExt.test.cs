
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

   [Theory]
   [InlineData("Iw.ant to add two numbers", 2)]
   [InlineData("I_w@!ant to subtract two numbers",4)]
   public void Should_contain_target_find_match(string input, int index)
   {
      var matches = input.GetWordEndings();
      Assert.True(matches.First().Index == index);
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
      Assert.Contains(target, sut.Select(x => x.Value).ToList());
   }
   
   [Theory]
   [InlineData("I want to add two numbers", "add", "add")]
   [InlineData("I want to suabtract two numbers", "subtract", "suabtract")]
   [InlineData("I want to perform subtraction", "subtract", "subtracti")]
   public void Should_contain_target_find_match(string phrase, string input, string target)
   {
      var matrix = phrase.ToFlatCharacterStringMatrix();
      var sut = matrix.Distinct().ToList().FilterLevenshteinMatch(input);
      Assert.Contains(target, sut.Select(x => x.Value).ToList());
   }
   
   [Theory]
   [InlineData("I want to multiply two numbers 58*39", "add", "add")]
   public void Should_no_match(string phrase, string input, string target)
   {
      var matrix = phrase.ToFlatCharacterStringMatrix();
      var sut = matrix.Distinct().ToList().FilterLevenshteinMatch(input);
      Assert.DoesNotContain(target, sut.Select(x => x.Value).ToList());
   }

   [Theory]
   [InlineData("aw", 0, "awesomes fruits", "awesomes")]
   [InlineData("aw", 1, " awesomes fruits", "awesomes")]
   [InlineData("frui",10, " awesomes fruits", "fruits")]
   [InlineData("frui",10, " awesomes fruits ", "fruits")]
   [InlineData("frui",9, "awesomes fruits brats", "fruits")]
   [InlineData("frui",10, "!awesomes fruits brats@", "fruits")]
   [InlineData("frui",10, "!awesomes fruits\nbrats@", "fruits")]
   [InlineData("frui",10, "!awesomes fruits_brats@", "fruits_brats@")]
   [InlineData("frui",10, "!awesomes Fruit$ brats@", "Fruit$")]
   public void Should_find_word(string phrase, int start, string origin, string expected) 
   {
      StringSegment stringSegment= new StringSegment(phrase, start, origin);
      var sut = stringSegment.GetWordFromOrigin();
      Assert.Equal(expected, sut);
   }

   [Theory]
   [InlineData("0123456789", "0123456789", true)]
   [InlineData("01234567xx", "0123456789", true)]
   [InlineData("0123456789", "01234567xx", true)]
   [InlineData("0123456789", "0123456789xx", true)]
   [InlineData("0123456789xx", "0123456789", true)]
   [InlineData("012345xxxx", "0123456789", false)]
   [InlineData("0123456789", "012345xxxx", false)]
   [InlineData("0123456", "0123456789", true)]
   [InlineData("0123456789", "0123456", true)]
   public void Should_find_match_when_tolerant(string phrase1, string phrase2, bool expected)
   {
      bool sut = phrase1.LevenshteinMatch(phrase2);
      Assert.True(sut == expected);
   }
}
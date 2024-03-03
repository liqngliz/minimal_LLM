namespace minimal.LLM.SemanticKernel.Test;

public class SemanticKernelTest
{
string text = 
@"1st Law of Thermodynamics - Energy cannot be created or destroyed.
2nd Law of Thermodynamics - For a spontaneous process, the entropy of the universe increases.
3rd Law of Thermodynamics - A perfect crystal at zero Kelvin has zero entropy.";

    [Fact]
    public async void Test1()
    {
        var sut = new LlmSemanticKernel();
        var res = await sut.summarize(text);
        Assert.False(string.IsNullOrEmpty(res));
    }
}
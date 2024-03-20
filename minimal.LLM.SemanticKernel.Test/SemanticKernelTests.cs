namespace minimal.LLM.SemanticKernel.Test;

[Collection("Sequential")]
public class SemanticKernelTest
{
string text = 
@"1st Law of Thermodynamics - Energy cannot be created or destroyed.
2nd Law of Thermodynamics - For a spontaneous process, the entropy of the universe increases.
3rd Law of Thermodynamics - A perfect crystal at zero Kelvin has zero entropy.";

    [Fact]
    public async void Should_Summarize()
    {
        var sut = new LlmSemanticKernel();
        var res = await sut.summarize(text);
        Assert.False(string.IsNullOrEmpty(res));
    }

    [Fact]
    public async void Should_Sqrt()
    {
        var sut = new LlmSemanticKernel();
        var res = await sut.sqrt(9);
        Assert.Equal("The sqaure root of 9 is 3", res);
    }

    [Fact]
    public async void Should_Complete()
    {
        var sut = new LlmSemanticKernel();
        var res = await sut.copilot("What is the square root of 24649?");
        Assert.NotNull(res);
    }

    [Fact]
    public async void Should_Plan()
    {
        var sut = new LlmSemanticKernel();
        var res = await sut.plannedCopilot("What is the square root of 24649?");
        //Assert.NotNull(res);
    }
}
namespace minimal.LLM.SemanticKernel.Test;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        var _sut = new LlmSemanticKernel();
        var res = _sut.init().Result;
        Assert.Equal(res.Count, 2);
    }
}
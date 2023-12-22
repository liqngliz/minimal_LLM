using Autofac;
using Configuration;
using Context;
using IoC;
using Run;

namespace ContextTest;
public class ContextTest 
{
    readonly IContext<LlamaInstance> _sut;

    public ContextTest () 
    {
        var modules = new IoCModule("config.json");
        _sut = modules.Container().Resolve<IContext<LlamaInstance>>();
    }

    [Fact]
    public async Task Should_Return_Llm_Params()
    {
        var llmParams = await _sut.Init();
        var modelParams = llmParams.ModelParams;
        var infParams = llmParams.InferenceParams;
        Assert.True(llmParams != null);
        Assert.True(modelParams.ContextSize == 4096);
        Assert.True(modelParams.Seed == 1337);
        Assert.True(modelParams.GpuLayerCount == 5);

        Assert.True(infParams.MaxTokens == 2048);
        Assert.True(infParams.Temperature == 0.8f);
        Assert.True(infParams.RepeatPenalty == 1.1f);
        Assert.True(llmParams.Prompt == File.ReadAllText("prompt.txt"));

    }
}
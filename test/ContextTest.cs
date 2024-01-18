using Autofac;
using Configuration;
using Context;
using IoC;
using Llm;
using Run;

namespace ContextTest;
public class ContextTest 
{
    readonly IContext<LlamaInstance> _sut;
    readonly Illm<IAsyncEnumerable<string>, string, LlamaInstance,bool> _sut2;

    public ContextTest () 
    {
        var modules = new IoCModule("config.json");
        _sut = modules.Container().Resolve<IContext<LlamaInstance>>();
        _sut2 = modules.Container().Resolve<Illm<IAsyncEnumerable<string>, string, LlamaInstance,bool>>();
        
    }

    [Fact]
    public async Task Should_Return_Llm_Params()
    {
        var llmParams = await _sut.Init();
        var modelParams = llmParams.ModelParams;
        var infParams = llmParams.InferenceParams;

        Assert.True(llmParams != null);
        Assert.True(modelParams.ContextSize == 2048);
        Assert.True(modelParams.Seed == 1337);
        Assert.True(modelParams.GpuLayerCount == 1);

        Assert.True(infParams.MaxTokens == 1024);
        Assert.True(infParams.Temperature == 0.8f);
        Assert.True(infParams.RepeatPenalty == 1.1f);
        Assert.True(llmParams.Prompt == File.ReadAllText("prompt.txt"));

        llmParams = _sut2.InferParams();
        modelParams = llmParams.ModelParams;
        infParams = llmParams.InferenceParams;

        Assert.True(llmParams != null);
        Assert.True(modelParams.ContextSize == 2048);
        Assert.True(modelParams.Seed == 1337);
        Assert.True(modelParams.GpuLayerCount == 1);

        Assert.True(infParams.MaxTokens == 1024);
        Assert.True(infParams.Temperature == 0.8f);
        Assert.True(infParams.RepeatPenalty == 1.1f);
        Assert.True(llmParams.Prompt == File.ReadAllText("prompt.txt"));

    }

    [Fact]
    public async Task Should_Return_Llm_Executor()
    {   
        var llmParams = await _sut.Init();
        var executor =  _sut2.Infer(llmParams.Prompt);
        Assert.True(executor is IAsyncEnumerable<string>);
        Assert.False(executor is List<string>);
        _sut2.Dispose();
    }
}
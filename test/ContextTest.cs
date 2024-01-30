using Autofac;
using Configuration;
using Context;
using IoC;
using Llm;
using Run;

namespace ContextTest;

[Collection("Sequential")]
public class ContextTest 
{
    readonly IContext<LlmContextInstance> _sut;
    readonly Illm<IAsyncEnumerable<string>, string, LlmContextInstance,bool> _sut2;

    public ContextTest () 
    {
        var modules = new IoCModule("config.json");
        _sut = modules.Container().Resolve<IContext<LlmContextInstance>>();
        _sut2 = modules.Container().Resolve<Illm<IAsyncEnumerable<string>, string, LlmContextInstance,bool>>();
        
    }

    [Fact]
    public async Task Should_Return_Llm_Params()
    {
        var llmParams = await _sut.Init();
        var modelParams = llmParams.ModelParams;
        var infParams = llmParams.InferenceParams;

        Assert.True(llmParams != null);
        Assert.True(modelParams.ContextSize != null);
        Assert.True(modelParams.Seed > 1);
        Assert.True(modelParams.GpuLayerCount >= 1);

        Assert.True(infParams.MaxTokens >= 0);
        Assert.True(infParams.Temperature >= 0.0f);
        Assert.True(infParams.RepeatPenalty >= 0.0f);
        Assert.True(llmParams.Prompt == File.ReadAllText("prompt.txt"));

        llmParams = _sut2.InferParams();
        modelParams = llmParams.ModelParams;
        infParams = llmParams.InferenceParams;

        Assert.True(llmParams != null);
        Assert.True(modelParams.ContextSize != null);
        Assert.True(modelParams.Seed > 1);
        Assert.True(modelParams.GpuLayerCount >= 1);

        Assert.True(infParams.MaxTokens >= 0);
        Assert.True(infParams.Temperature >= 0.0f);
        Assert.True(infParams.RepeatPenalty >= 0.0f);
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
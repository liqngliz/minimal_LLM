using Autofac;
using Configuration;
using Context;
using IoC;
using Run;

namespace IoCTest;

public class IoCTest
{   
    readonly IModule<Config> _sut;
    public IoCTest()
    {
        _sut = new IoCModule("config.json");
    }

    [Fact]
    public void Should_Return_Config()
    {
        Assert.Equal(_sut.Configuration(), new Config(4096, 1337, 5, 2048, 0.8f, 1.1f, "python-gguf-model-q4_k_m.bin","prompt.txt"));
    }

    [Theory]
    [InlineData(typeof(IContext<LlamaInstance>),typeof(LlamaSharpContext))]
    [InlineData(typeof(IRun), typeof(RunLlama))]
    public void Should_Resolve_As(Type interfaceType, Type classType)
    {
        var res = _sut.Container().Resolve(interfaceType);
        Assert.True(res.GetType() == classType);
    }
}
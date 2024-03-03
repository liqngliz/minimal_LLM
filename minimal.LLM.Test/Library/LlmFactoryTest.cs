using Autofac;
using Configuration;
using Context;
using IoC;
using LLama;
using LLama.Abstractions;
using Factory;

namespace LlmFactoryTest;

[Collection("Sequential")]
public class LlmFactoryTest
{
    readonly IModule<Config> _IoC;
    private IFactory<ILLamaExecutor> _sut;
  
    public LlmFactoryTest()
    {
        _IoC = new IoCModule("config.json");
    }

    [Theory]
    [InlineData(typeof(InteractiveExecutor))]
    [InlineData(typeof(InstructExecutor))]
    [InlineData(typeof(StatelessExecutor))]
    public async void Should_resolve_executor(Type expected)
    {   var context = await _IoC.Container().Resolve<IContext<LlmContextInstance>>().Init();
        _sut = new LlmFactory(context);

        var actual = _sut.Make(expected);
        Assert.IsType(expected, actual);

    }

}

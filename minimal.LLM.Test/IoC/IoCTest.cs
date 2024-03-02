using System.Reflection;
using Autofac;
using Configuration;
using Context;
using IoC;
using Llm;
using Reasoners;

namespace IoCTest;

[Collection("Sequential")]
public class IoCTest
{   
    readonly IModule<Config> _sut;
    private string _assPath;
    readonly string _assParentPath;
    public IoCTest()
    {
        _sut = new IoCModule("config.json");
        _assPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        _assParentPath = new DirectoryInfo(_assPath).Parent.FullName;
    }

    [Fact]
    public void Should_Return_Config()
    {
        Assert.NotNull(_sut.Configuration());
    }

    [Theory]
    [InlineData(typeof(IContext<LlmContextInstance>),typeof(LlamaSharpContext))]
    [InlineData(typeof(Illm<IAsyncEnumerable<string>, string, LlmContextInstance,bool>), typeof(LlmInstance))]
    [InlineData(typeof(IReasoner<Reasoning, ReasonerTemplate>), typeof(LlmReasoner))]
    public void Should_Resolve_As(Type interfaceType, Type classType)
    {
        var res = _sut.Container().Resolve(interfaceType);
        Assert.True(res.GetType() == classType);
    }

    [Theory]
    [InlineData("./config.json")]
    [InlineData(".\\config.json")]
    public void Should_Replace_Start_With_Absolute_Path(string path)
    {
        var absPath = (Location) path;
        Assert.True(absPath.ToString().Contains(_assPath));
    }

    [Theory]
    [InlineData("../config.json")]
    [InlineData("..\\config.json")]
    public void Should_Replace_Start_With_Absolute_Path_Parent(string path)
    {
        var absPath = (Location) path;
        Assert.True(absPath.ToString().Contains(_assParentPath));
    }
}
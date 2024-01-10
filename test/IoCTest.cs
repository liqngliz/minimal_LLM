using System.ComponentModel;
using System.Reflection;
using Autofac;
using Configuration;
using Context;
using IoC;
using llm;
using Run;

namespace IoCTest;

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
        Assert.Equal(_sut.Configuration(), new Config(4096, 1337, 5, 2048, 0.8f, 1.1f, "mistral-7b-instruct-v0.2.Q4_K_M.gguf","prompt.txt"));
    }

    [Theory]
    [InlineData(typeof(IContext<LlamaInstance>),typeof(LlamaSharpContext))]
    [InlineData(typeof(IRun), typeof(RunLlama))]
    [InlineData(typeof(Illm<IAsyncEnumerable<string>, string, LlamaInstance>), typeof(LlamaSharpLlm))]
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
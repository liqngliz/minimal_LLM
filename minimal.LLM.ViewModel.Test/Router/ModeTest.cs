

using Microsoft.SemanticKernel;
using Router;

namespace ModeTest;

public class ModeSingletonTest
{
    readonly IModeSingleton _sut;

    public ModeSingletonTest()
    {
        _sut = ModeSingleton.Instance;
        _sut.Init("toggle_use_agent", "Using agent routing:");
    }

    [Fact]
    public void should_toggle_when_key_used()
    {   
        var expected = "Using agent routing: True";
        var res = _sut.CheckMode("I would like to toggle_use_agent");
        Assert.Equal(expected, res);
        
        expected = null;
        res = _sut.CheckMode("I want to getfiles named text.txt");
        Assert.Equal(expected, res);
        var sut2 = ModeSingleton.Instance;
        expected = "Using agent routing: False";
        res = sut2.CheckMode("toggle_use_agent");
        Assert.Equal(expected, res);
    }
}
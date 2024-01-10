using System.ComponentModel;
using System.Reflection;
using Autofac;
using Configuration;
using Context;
using IoC;
using llm;
using Run;

namespace LlmTest;

public class LlmTest
{   
    readonly Illm<IAsyncEnumerable<string>, string, LlamaInstance> _sut;

    public LlmTest()
    {   
        var modules = new IoCModule("config.json");
        _sut = modules.Container().Resolve<Illm<IAsyncEnumerable<string>, string, LlamaInstance>>();
    }

    [Fact]
    public async void Should_Infer_Prompt()
    {
        var llama = _sut.InferParams();
        var prompt = @"Transcript of a dialog, where the User interacts with an assistant named AIssistant.
        User: Hello, AIssistant.
        AIssistant:Hello. How may I help you today with you questions?
        User:";

        string res = "";

        await foreach (var text in _sut.Infer(prompt)) 
        {   
                res = res + text;
        }

        Assert.Equal(res, "");

        prompt = @"what is 2 + 2, can you only answer with the numerical result?";

        await foreach (var text in _sut.Infer(prompt)) 
        {   
                res = res + text;
        }

        Assert.NotEqual(res, "");
        Assert.True(res.Contains("4"));

        prompt = @"what is your name?";
        res = "";
        
        await foreach (var text in _sut.Infer(prompt)) 
        {   
                res = res + text;
        }
        Assert.NotEqual(res, "");
        Assert.True(res.Contains("AIssistant"));
        
    }

}
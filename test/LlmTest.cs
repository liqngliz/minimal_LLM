using Autofac;
using Context;
using IoC;
using Llm;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Engine.ClientProtocol;

namespace LlmTest;

public class LlmTest
{   
    readonly Illm<IAsyncEnumerable<string>, string, LlamaInstance, bool> _sut;

    public LlmTest()
    {   
        var modules = new IoCModule("config.json");
        _sut = modules.Container().Resolve<Illm<IAsyncEnumerable<string>, string, LlamaInstance, bool>>();
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

        string context = utils.testLargeContext;

        prompt = $"I have a stock portfolio where I invested 50% in crypto currencies 25% in a bond etf and 25% in equities etf, now my crypto currencies are worth 900$ while the bond etf is worth 280$ and equities etf 300$ what should I do according to {utils.testLargeContext}";
        
        res = "";
        
        await foreach (var text in _sut.Infer(prompt)) 
        {   
                res = res + text;
        }
        Assert.NotEqual(res, "");
        Assert.True(res.ToLowerInvariant().Contains("rebalance"));
    }

}

public static class utils
{
        public static string testLargeContext = @"How To Rebalance Your Portfolio
The optimal frequency of portfolio rebalancing depends on your transaction costs, personal preferences, and tax considerations, including what type of account you are selling from and whether your capital gains or losses will be taxed at a short-term versus long-term rate. It also differs based on your age. For example, if you are relatively young, say in your 20s and 30s, you might not want to rebalance your portfolio as frequently as when you are nearing retirement and need to maximize your gains. Usually, about once a year is sufficient; however, if some assets in your portfolio haven't experienced a large appreciation within the year, longer periods may also be appropriate.";
}
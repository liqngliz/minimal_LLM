using Context;
using IoC;
using Autofac;
using Reasoners;

namespace LlmClassifierTest;

[Collection("Sequential")]
public class LlmClassifierTest
{
    readonly IReasoner<string, Classify> _classificationReasoner;
    
    public LlmClassifierTest()
    {
        var modules = new IoCModule("config.json");
        _classificationReasoner = modules.Container().Resolve<IReasoner<string,Classify>>();
    }

    [Fact]
    public async void should_classify_as_state()
    {
        var categories = new string[]{"Continent", "Country", "Province", "Canton", "Region", "State", "City", "Village"};
        var content = "Virginia";
        Classify input = new Classify(content, "Other", categories);
        var res = await _classificationReasoner.Reason(input);
        Assert.Equal("State", res);
    }

    [Fact]
    public async void should_classify_as_Rg()
    {
        var categories = new string[]{"Conti", "Ctry", "Rg", "Cty", "Vg"};
        var categoriesDesc = new string[]{"A continent", "A country", "A region, province, state, canton or equivalent", "A city, district or similar", "A village"};
        var content = "Vaud";
        Classify input = new Classify(content, "Vg", categories, categoriesDesc);
         var res = await _classificationReasoner.Reason(input);
        Assert.Equal("Rg", res);
    }
}
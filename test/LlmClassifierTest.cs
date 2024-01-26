using Context;
using IoC;
using Autofac;
using Reasoners;

namespace LlmClassifierTest;

[Collection("Sequential")]
public class LlmClassifierTest
{
    readonly IReasoner<List<string>, Classify> _classificationReasoner;
    
    public LlmClassifierTest()
    {
        var modules = new IoCModule("config.json");
        _classificationReasoner = modules.Container().Resolve<IReasoner<List<string>, Classify>>();
    }

    [Theory]
    [InlineData(
        new string[]{"Continent", "Country", "Province", "Canton", "Region", "State", "City", "Village", "Other"}, 
        "Virginia", 
        new string[]{"State"}, 
        new string[]{"Village, Continent, Country, Canton, City, Village, Other"}
    )]

    [InlineData(
        new string[]{"Denial Of Service", "Hack", "End Of Life", "Program Vulnerability", "Transient Incident", "Network Issue", "Program Bug", "Other"}, 
        "When trying to update a field in SAP I get an error that says Java Rest Client error 500", 
        new string[]{"Program Bug"}, 
        new string[]{"Denial Of Service", "Hack", "End Of Life", "Program Vulnerability", "Transient Incident", "Network Issue"}
    )]

    [InlineData(
        new string[]{"Denial Of Service", "Hack", "End Of Life", "Program Vulnerability", "Transient Incident", "Network Issue", "Program Bug", "Other"}, 
        "When trying to update a field in SAP I get an error that says Login Failed", 
        new string[]{"Network Issue"}, 
        new string[]{"Denial Of Service", "Hack", "End Of Life", "Program Vulnerability", "Transient Incident", "Program Bug"}
    )]

    public async void should_classify_as_state(string[] categories, string content, string[] positives, string[] negatives)
    {
        Classify input = new Classify(content, categories);
        var res = await _classificationReasoner.Reason(input);
        foreach(string positive in positives) Assert.Contains(positive, res);
        foreach(string negative in negatives) Assert.DoesNotContain(negative, res);
    }

    [Theory]
    [InlineData(
        new string[]{"Conti", "Ctry", "Rg", "Cty", "Vg", "Oth"}, 
        new string[]{"A continent", "A country", "A region, province, state, canton or equivalent", "A city, district or similar", "A village", "Belongs to none of the specified categories"},
        "Virginia", 
        "Rg"
    )]
    [InlineData(
        new string[]{"Conti", "Ctry", "Rg", "Cty", "Vg", "Oth"}, 
        new string[]{"A continent", "A country", "A region, province, state, canton or equivalent", "A city, district or similar", "A village", "Belongs to none of the specified categories"},
        "Vaud", 
        "Rg"
    )]

    [InlineData(
        new string[]{"Conti", "Ctry", "Rg", "Cty", "Vg", "Oth"}, 
        new string[]{"A continent", "A country", "A region, province, state, canton or equivalent", "A city, district or similar", "A village", "Belongs to none of the specified categories"},
        "fish sticks", 
        "Oth"
    )]

    [InlineData(
        new string[]{"2356sdragh", "serdheasraerjsrnm", "weart43qraseh", "dyfrhjn23qt5dfbh", "34q6qewahae", "346q34dafjajer"}, 
        new string[]{"A continent", "A country", "A region, province, state, canton or equivalent", "A city, district or similar", "A village", "Belongs to none of the specified categories"},
        "Antartica", 
        "2356sdragh"
    )]

    public async void should_classify_as_Rg(string[] categories, string[] categoriesDesc, string content ,string result)
    {
        Classify input = new Classify(content, categories, categoriesDesc);
         var res = await _classificationReasoner.Reason(input);
        Assert.Contains(result, res);
    }
}
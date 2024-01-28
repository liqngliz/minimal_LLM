using Context;
using IoC;
using Autofac;
using Reasoners;
using UtilsExt;

namespace LlmClassifierTest;

[Collection("Sequential")]
public class LlmClassifierTest
{
    readonly IReasoner<List<string>, ClassifyL1> _classificationReasonerL1;
    readonly IReasoner<List<string>, ClassifyL2> _classificationReasonerL2;
    
    public LlmClassifierTest()
    {
        var modules = new IoCModule("config.json");
        _classificationReasonerL1 = modules.Container().Resolve<IReasoner<List<string>, ClassifyL1>>();
        _classificationReasonerL2 = modules.Container().Resolve<IReasoner<List<string>, ClassifyL2>>();
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
        new string[]{"Denial Of Service", "Hack", "End Of Life", "Program Vulnerability", "Transient Incident", "Network Issue", "Program Bug", "Authentication","Other"}, 
        "When trying to update a field in SAP I get an error that says Login Failed", 
        new string[]{"Authentication"}, 
        new string[]{"Denial Of Service", "Hack", "End Of Life", "Program Vulnerability", "Transient Incident", "Program Bug"}
    )]

    public async void should_classify_as_state(string[] categories, string content, string[] positives, string[] negatives)
    {
        ClassifyL1 input = new ClassifyL1(content, categories);
        var res = await _classificationReasonerL1.Reason(input);
        foreach(string positive in positives) Assert.Contains(positive, res);
        foreach(string negative in negatives) Assert.DoesNotContain(negative, res);
    }

    [Theory]
    [InlineData(
        new string[]{"Conti", "Ctry", "Rg", "Cty", "Vg", "Oth"}, 
        new string[]{"A continent", "A country", "A region, province, state, canton or equivalent", "A city, district or similar", "A village", "Belongs to none of the specified categories"},
        "Virginia", 
        "Rg",
        new string[] {"Vg", "Oth", "Conti"}
    )]
    [InlineData(
        new string[]{"Conti", "Ctry", "Rg", "Cty", "Vg", "Oth"}, 
        new string[]{"A continent", "A country", "A region, province, state, canton or equivalent", "A city, district or similar", "A village", "Belongs to none of the specified categories"},
        "The valley of joux", 
        "Rg",
        new string[] {"Vg", "Oth", "Conti", "Cty"}
    )]

    [InlineData(
        new string[]{"Conti", "Ctry", "Rg", "Cty", "Vg", "Oth"}, 
        new string[]{"A continent", "A country", "A region, province, state, canton or equivalent", "A city, district or similar", "A village", "Belongs to none of the specified categories"},
        "fish sticks", 
        "Oth",
        new string[] {"Conti", "Ctry", "Rg", "Cty", "Vg",}
    )]

    [InlineData(
        new string[]{"2356sdragh", "serdheasraerjsrnm", "weart43qraseh", "dyfrhjn23qt5dfbh", "34q6qewahae", "346q34dafjajer"}, 
        new string[]{"A continent", "A country", "A region, province, state, canton or equivalent", "A city, district or similar", "A village", "Belongs to none of the specified categories"},
        "Antartica", 
        "2356sdragh",
        new string[] {"serdheasraerjsrnm", "weart43qraseh", "dyfrhjn23qt5dfbh", "34q6qewahae", "346q34dafjajer"}
    )]

    public async void should_classify_as_Rg(string[] categories, string[] categoriesDesc, string content ,string result, string[] negatives)
    {   
        Category[] cats = categories.WithIndex().Select(x => new Category(x.item, categoriesDesc[x.index]) ).ToArray();
        ClassifyL2 input = new ClassifyL2(content, cats);
         var res = await _classificationReasonerL2.Reason(input);
        Assert.Contains(result, res);
    }
}
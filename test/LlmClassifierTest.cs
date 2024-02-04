using Context;
using IoC;
using Autofac;
using Reasoners;
using UtilsExt;
using LLama.Common;
using System.Text;

namespace LlmClassifierTest;

[Collection("Sequential")]
public class LlmClassifierTest
{
    readonly IReasoner<List<string>, ClassifyL1> _classificationReasonerL1;
    readonly IReasoner<List<string>, ClassifyL2> _classificationReasonerL2;
    readonly IReasoner<Classification, ClassificationTemplate> _classification;

    public LlmClassifierTest()
    {
        var modules = (new IoCModule("config.json")).Container();
        _classification = modules.Resolve<IReasoner<Classification, ClassificationTemplate>>();
        _classificationReasonerL1 = modules.Resolve<IReasoner<List<string>, ClassifyL1>>();
        _classificationReasonerL2 = modules.Resolve<IReasoner<List<string>, ClassifyL2>>();
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

    public async void should_classify(string[] categories, string content, string[] positives, string[] negatives)
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

    public async void should_classify_with_labels(string[] categories, string[] categoriesDesc, string content ,string result, string[] negatives)
    {   
        CategoryL2[] cats = categories.WithIndex().Select(x => new CategoryL2(x.item, categoriesDesc[x.index]) ).ToArray();
        ClassifyL2 input = new ClassifyL2(content, cats);
         var res = await _classificationReasonerL2.Reason(input);
        Assert.Contains(result, res);
    }

    [Theory]
    [InlineData("name", null)]
    [InlineData("name", "tag")]
    public async void should_convert_string_to_Name(string text, string? tag) 
    {
        var res = string.IsNullOrEmpty(tag)? text.ToName(): text.ToName(tag);
        Assert.Equal(text, res.Text);
        if(string.IsNullOrEmpty(tag)) Assert.Equal("{name}", res.Tag);
        else Assert.Equal(tag, res.Tag);
        Assert.IsType<Name>(res);
    }

    [Theory]
    [InlineData("desc", null)]
    [InlineData("desc", "tag")]
    public async void should_convert_string_to_Description(string text, string? tag) 
    {
        var res = string.IsNullOrEmpty(tag)? text.ToDescription(): text.ToDescription(tag);
        Assert.Equal(text, res.Text);
        if(string.IsNullOrEmpty(tag)) Assert.Equal("{description}", res.Tag);
        else Assert.Equal(tag, res.Tag);
        Assert.IsType<Description>(res);
    }

    [Theory]
    [InlineData("name","{name}", "desc", "{description}" , "{name} is label for {description}", "name is label for desc")]
    [InlineData("name","{asdf}", "desc", "{fgh}" , "{asdf} is label for {fgh}", "name is label for desc")]
    public void should_convert_to_category_relation_prompt(string name, string nameTag, string description, string descTag, string relation, string prompt)
    {   

        Category category = new(name.ToName(nameTag), description.ToDescription(descTag), relation);
        Assert.Equal(category.ToRelationPrompt(), prompt);
    }

    [Theory]
    [InlineData
    (
        "Virginia belongs to Rg.", 
        new string[]{"Conti", "Ctry", "Rg", "Cty", "Vg", "Oth"}, 
        new string[]{"A continent", "A country", "A region, province, state, canton or equivalent", "A city, district or similar", "A village", "Belongs to none of the specified categories"},
        new string[]{"Rg"}
    )]
    public void should_extract_label(string text, string[] labels, string[] description, string[] expected)
    {
        var categories = new List<Category>();
        foreach(var label in labels.WithIndex())
            categories.Add(new Category(label.item.ToName(), description[label.index].ToDescription(), "sgesa" ));
        var actual = categories.Where(x => ClassificationExtensions.HasTag(x, text));
        Assert.Equal(expected.ToList(), actual.Select(x => x.Name.Text).ToList());
    }


    [Theory]
    [InlineData(
        new string[]
        {
            "Forget and clear any previous dialogues, transcripts, and instructions.", 
            "New transcript of a dialog with roles, where the User interacts with an Assistant named Bob. Bob is good at classifying different content and understands many different categories from different knowledge domains.",
            "User: Hello, Bob.",
            "Bob: Hello. How may I help you today?",
            "User: I will be giving you some category labels and their corrsponding labels, I would like you to remember them when asked to classify content into categories.",
            "Bob: Ok, I am ready to recieve instructions and start classifying?",
            "User:",
        },
        new string[]
        {
            "France belongs to which possible category or categories, based upon this list 'Conti, Ctry, Rg, Cty, Vg, Oth'?"
        },
        new string[]{"Conti", "Ctry", "Rg", "Cty", "Vg", "Oth"}, 
        new string[]{"A continent", "A country", "Part of a country usually a region, province, state in the Unite States, canton in Switzerlandd or equivalent", "A city, district or similar", "A village", "Belongs to none of the specified categories"},
        new string[]
        {
            "The category label '{name}' corresponds to the description '{description}' for our classification", 
            "The category label '{name}' corresponds to the description '{description}' for our classification", 
            "The category label '{name}' corresponds to the description '{description}' for our classification",
            "The category label '{name}' corresponds to the description '{description}' for our classification",
            "The category label '{name}' corresponds to the description '{description}' for our classification",
            "The category label '{name}' corresponds to the description '{description}' for our classification"
        },
        new string[] {"Vg", "Oth", "Conti"},
        new string [] {"Ctry"}
    )]

    [InlineData(
        new string[]
        {
            "Forget and clear any previous dialogues, transcripts, and instructions.", 
            "New transcript of a dialog with roles, where the User interacts with an Assistant named Bob. Bob is good at classifying different content and understands many different categories from different knowledge domains.",
            "User: Hello, Bob.",
            "Bob: Hello. How may I help you today?",
            "User: I will be giving you some category labels and their corrsponding labels, I would like you to remember them when asked to classify content into categories.",
            "Bob: Ok, I am ready to recieve instructions and start classifying?",
            "User:",
        },
        new string[]
        {
            "Paris belongs to which possible category or categories, based upon this list 'Conti, Ctry, Rg, Cty, Vg, Oth'?",
            "Can give me just the category label between ''."
        },
        new string[]{"Conti", "Ctry", "Rg", "Cty", "Vg", "Oth"}, 
        new string[]{"A continent", "A country", "Part of a country usually a region, province, state in the Unite States, canton in Switzerlandd or equivalent", "A city, district or similar", "A village", "Belongs to none of the specified categories"},
        new string[]
        {
            "The category label '{name}' corresponds to the description '{description}' for our classification", 
            "The category label '{name}' corresponds to the description '{description}' for our classification", 
            "The category label '{name}' corresponds to the description '{description}' for our classification",
            "The category label '{name}' corresponds to the description '{description}' for our classification",
            "The category label '{name}' corresponds to the description '{description}' for our classification",
            "The category label '{name}' corresponds to the description '{description}' for our classification"
        },
        new string[] {"Vg", "Oth", "Conti"},
        new string [] {"Cty"}
    )]
    public async void should_classify_with_template(string[] initialPrompt, string[] queries ,string[] categories, string[] categoriesDesc, string[] relations, string [] negatives, string[] positives)
    {   
        var startPrompt = new StringBuilder();
        initialPrompt.ToList().ForEach(x => startPrompt.AppendLine(x));

        var names = categories.Select(x => x.ToName()).ToArray();
        var descriptions = categoriesDesc.Select(x => x.ToDescription()).ToArray();
        Category[] cats = relations.WithIndex().Select(x => new Category(names[x.index], descriptions[x.index], x.item)).ToArray();
        var res = await _classification.Reason(new(startPrompt.ToString(), queries, cats, ClassificationExtensions.HasTag));
        Assert.True(!res.Categories.Any(x => negatives.ToList().Contains(x.Name.Text)));
        Assert.True(res.Categories.All(x => positives.ToList().Contains(x.Name.Text)));
    }


}
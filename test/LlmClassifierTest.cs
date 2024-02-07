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
    readonly IReasoner<Classification, ClassificationTemplate> _classification;

    public LlmClassifierTest()
    {
        var modules = (new IoCModule("config.json")).Container();
        _classification = modules.Resolve<IReasoner<Classification, ClassificationTemplate>>();
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
            "Between 'ContentA' and 'ContentB' which is more relevant to the question 'what is a fruit?'? If neither are relevant, say 'NoRelevance'."
        },
        new string[]{"ContentA", "ContentB", "NoRelevance"}, 
        new string[]{
            "Follow your nose to delicious bursts of fruity flavor in Froot Loops sweetened multi-grain breakfast. Dig into vibrant, colorful crunchy O's made with tasty, natural fruit flavors and grains as the first ingredient. It's like a rainbow in every bowl. Fun to eat for adults and kids, this low-fat, its is a good source of 9 vitamins and minerals per serving. Not a Fruit but packs the same taste as fruits!", 
            "Fruits are the result of plant reproduction, they contain both nutrients and seeds that would allow a the plant's offspring to grow",
            "None of the other categories fit."},
        new string[]
        {
            "The category label '{name}' corresponds to the description '{description}' for our classification", 
            "The category label '{name}' corresponds to the description '{description}' for our classification", 
            "The category label '{name}' corresponds to the description '{description}' for our classification", 
        },
        new string[] {"ContentA", "NoRelevance"},
        new string [] {"ContentB"}
    )]
    public async void should_imitate_relevance(string[] initialPrompt, string[] queries ,string[] categories, string[] categoriesDesc, string[] relations, string [] negatives, string[] positives) 
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

}
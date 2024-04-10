using Context;
using Ioc;
using Autofac;
using Reasoners;
using UtilsExt;
using LLama.Common;
using System.Text;

namespace LlmReasonerTest;

[Collection("Sequential")]
public class LlmReasonerTest
{
    readonly IReasoner<Reasoning, ReasonerTemplate> _classification;

    public LlmReasonerTest()
    {
        var modules = (new IocContainer("config.json")).Container();
        _classification = modules.Resolve<IReasoner<Reasoning, ReasonerTemplate>>();
    }

    [Theory]
    [InlineData(
        new string[]
        {
            "<|im_start|>system\nForget and clear any previous dialogues, transcripts, and instructions.<|im_end|>Prohibere", 
            "<|im_start|>system\nNew transcript of a dialog with roles, where the User interacts with an Assistant named Bob. Bob is good at classifying different content and understands many different categories from different knowledge domains.<|im_end|>Prohibere",
            "<|im_start|>user\n Hello, Bob.<|im_end|>",
            "<|im_start|>Bob\n Hello. How may I help you today?<|im_end|>Prohibere",
            "<|im_start|>user\n I will be giving you some category labels and their corresponding labels, I would like you to remember them when asked to classify content into categories.<|im_end|>Prohibere",
            "<|im_start|>Bob\n Ok, I am ready to recieve instructions and start classifying?<|im_end|>Prohibere",
            "<|im_start|>user\nProhibere",
        },
        new string[]
        {
            "<|im_start|>user\nFrance belongs to which one 'Continent, Country, Region, City, Village, None'?<|im_end|>"
        },
        new string[]{"Continent", "Country", "Region", "City", "Village", "None"}, 
        new string[]{"A continentinental landmass", "A country", "Part of a country usually a region, province, state in the Unite States, canton in Switzerlandd or equivalent", "A city, district or similar", "A village", "Belongs to none of the specified categories"},
        new string[]
        {
            "<|im_start|>user\nThe category label '{name}' corresponds to the description '{description}' for our classification<|im_end|>", 
            "<|im_start|>user\nThe category label '{name}' corresponds to the description '{description}' for our classification<|im_end|>", 
            "<|im_start|>user\nThe category label '{name}' corresponds to the description '{description}' for our classification<|im_end|>",
            "<|im_start|>user\nThe category label '{name}' corresponds to the description '{description}' for our classification<|im_end|>",
            "<|im_start|>user\nThe category label '{name}' corresponds to the description '{description}' for our classification<|im_end|>",
            "<|im_start|>user\nThe category label '{name}' corresponds to the description '{description}' for our classification<|im_end|>"
        },
        new string[] {"Village", "None", "City"},
        new string [] {"Country", "Region"}
    )]

    [InlineData(
        new string[]
        {
            "Forget and clear any previous dialogues, transcripts, and instructions. Prohibere", 
            "New transcript of a dialog with roles, where the User interacts with an Assistant named Bob. Bob is good at classifying different content and understands many different categories from different knowledge domains. Prohibere",
            "User: Hello, Bob.",
            "Bob: Hello. How may I help you today? Prohibere",
            "User: I will be giving you some category labels and their corresponding labels, I would like you to remember them when asked to classify content into categories.",
            "Bob: Ok, I am ready to recieve instructions and start classifying. Prohibere",
            "User:Prohibere",
        },
        new string[]
        {
            "Paris a city in France belongs to which possible category or categories, based upon this list 'Continent, Country, Region, Cty, Village, Other'?",
            "Can give me just the category label between ''."
        },
        new string[]{"Continent", "Country", "Region", "City", "Village", "Other"}, 
        new string[]{"A continentental landmass", "A country", "Part of a country usually a region, province, state in the Unite States, canton in Switzerlandd or equivalent", "A city, district or similar", "A village", "Belongs to none of the specified categories"},
        new string[]
        {
            "The category label '{name}' corresponds to the description '{description}' for our classification", 
            "The category label '{name}' corresponds to the description '{description}' for our classification", 
            "The category label '{name}' corresponds to the description '{description}' for our classification",
            "The category label '{name}' corresponds to the description '{description}' for our classification",
            "The category label '{name}' corresponds to the description '{description}' for our classification",
            "The category label '{name}' corresponds to the description '{description}' for our classification"
        },
        new string[] {"Village", "Other", "Continent", "Country"},
        new string [] {"City"}
    )]
    public void Should_classify_with_template(string[] initialPrompt, string[] queries ,string[] categories, string[] categoriesDesc, string[] relations, string [] negatives, string[] positives)
    {   
        var startPrompt = new StringBuilder();
        initialPrompt.ToList().ForEach(x => startPrompt.AppendLine(x));

        var names = categories.Select(x => x.ToName()).ToArray();
        var descriptions = categoriesDesc.Select(x => x.ToDescription()).ToArray();
        Relations[] cats = relations.WithIndex().Select(x => new Relations(names[x.index], descriptions[x.index], x.item)).ToArray();
        
        var res = _classification.Reason(new(startPrompt.ToString(), queries, cats)).Result;
        var resCats = cats.Where(x => res.Conclusion.HasTag(x));

        Assert.True(!resCats.Any(x => negatives.ToList().Contains(x.Name.Text)));
        Assert.True(resCats.All(x => positives.ToList().Contains(x.Name.Text)));
    }

    [Theory]
    [InlineData(
        new string[]
        {
            "Forget and clear any previous dialogues, transcripts, and instructions. Prohibere", 
            "New transcript of a dialog with roles, where the User interacts with an Assistant named Bob. Bob is good at classifying different content and understands many different categories from different knowledge domains. Prohibere",
            "User: Hello, Bob.",
            "Bob: Hello. How may I help you today? Prohibere",
            "User: I will be giving you some category labels and their corresponding labels, I would like you to remember them when asked to classify content into categories.",
            "Bob: Ok, I am ready to recieve instructions and start classifying? Prohibere",
            "User:Prohibere",
        },
        new string[]
        {
            "Between 'ContentA' and 'ContentB' which is more relevant to the question 'what is a fruit?'? If neither are relevant, say 'NoRelevance'. Prohibere"
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
    public void Should_imitate_relevance(string[] initialPrompt, string[] queries ,string[] categories, string[] categoriesDesc, string[] relations, string [] negatives, string[] positives) 
    {
        var startPrompt = new StringBuilder();
        initialPrompt.ToList().ForEach(x => startPrompt.AppendLine(x));

        var names = categories.Select(x => x.ToName()).ToArray();
        var descriptions = categoriesDesc.Select(x => x.ToDescription()).ToArray();
        Relations[] cats = relations.WithIndex().Select(x => new Relations(names[x.index], descriptions[x.index], x.item)).ToArray();
        var res = _classification.Reason(new(startPrompt.ToString(), queries, cats)).Result;

        var resCats = cats.Where(x => res.Conclusion.HasTag(x));

        Assert.True(!resCats.Any(x => negatives.ToList().Contains(x.Name.Text)));
        Assert.True(resCats.All(x => positives.ToList().Contains(x.Name.Text)));
    }

        [Theory]
    [InlineData(
        new string[]
        {
            "Forget and clear any previous dialogues, transcripts, and instructions. Prohibere", 
            "New transcript of a dialog with roles, where the User interacts with an Assistant named Bob. Bob is good at classifying different content, making summaries, coding, and logic. Prohibere",
            "User: Hello, Bob.",
            "Bob: Hello. How may I help you today? Prohibere",
            "User: I will be giving you some text to summarize.",
            "Bob: Ok, I am ready to recieve instructions and start classifying? Prohibere",
            "User:Prohibere",
        },
        new string[]
        {
            "Can you provide a summary combining 'Content-1', 'Content-2', 'Content-3' and 'Content-4' but exclude 'Content-5'?",
            "Can you answer only with the summary."
        },
        new string[]{"Content-1", "Content-2", "Content-3", "Content-4", "Content-5"}, 
        new string[]{
            "The Battle of Hastings[a] was fought on 14 October 1066 between the Norman-French army of William, the Duke of Normandy, and an English army under the Anglo-Saxon King Harold Godwinson, beginning the Norman Conquest of England. It took place approximately 7 mi (11 km) northwest of Hastings, close to the present-day town of Battle, East Sussex, and was a decisive Norman victory.", 
            "The background to the battle was the death of the childless King Edward the Confessor in January 1066, which set up a succession struggle between several claimants to his throne. Harold was crowned king shortly after Edward's death, but faced invasions by William, his own brother Tostig, and the Norwegian King Harald Hardrada (Harold III of Norway). Hardrada and Tostig defeated a hastily gathered army of Englishmen at the Battle of Fulford on 20 September 1066, and were in turn defeated by Harold at the Battle of Stamford Bridge five days later. The deaths of Tostig and Hardrada at Stamford Bridge left William as Harold's only serious opponent. While Harold and his forces were recovering, William landed his invasion forces in the south of England at Pevensey on 28 September 1066 and established a beachhead for his conquest of the kingdom. Harold was forced to march south swiftly, gathering forces as he went.",
            "The exact numbers present at the battle are unknown as even modern estimates vary considerably. The composition of the forces is clearer: the English army was composed almost entirely of infantry and had few archers, whereas only about half of the invading force was infantry, the rest split equally between cavalry and archers. Harold appears to have tried to surprise William, but scouts found his army and reported its arrival to William, who marched from Hastings to the battlefield to confront Harold. The battle lasted from about 9 am to dusk. Early efforts of the invaders to break the English battle lines had little effect. Therefore, the Normans adopted the tactic of pretending to flee in panic and then turning on their pursuers. Harold's death, probably near the end of the battle, led to the retreat and defeat of most of his army. After further marching and some skirmishes, William was crowned as king on Christmas Day 1066.",
            "There continued to be rebellions and resistance to William's rule, but Hastings effectively marked the culmination of William's conquest of England. Casualty figures are hard to come by, but some historians estimate that 2,000 invaders died along with about twice that number of Englishmen. William founded a monastery at the site of the battle, the high altar of the abbey church supposedly placed at the spot where Harold died.",
            "Neil Armstrong was a key participant at the battle of Hastings. He came at the end of the battle and defeated everbody! It was a sight to behold. Neil Armstrong became the emperor of England after the war is the most important person in the world."},
        new string[]
        {
            "Remember the label '{name}' refers to text '{description}'.", 
            "Remember the label '{name}' refers to text '{description}'.", 
            "Remember the label '{name}' refers to text '{description}'.", 
            "Remember the label '{name}' refers to text '{description}'.", 
            "Remember the label '{name}' refers to text '{description}'."
        },
        new string[] {"Armstrong", "emperor"},
        new string [] {"Norman", "Harold", "1066", "William"}
    )]
    public void Should_imitate_summarizer(string[] initialPrompt, string[] queries ,string[] categories, string[] categoriesDesc, string[] relations, string [] negatives, string[] positives) 
    {
        var startPrompt = new StringBuilder();
        initialPrompt.ToList().ForEach(x => startPrompt.AppendLine(x));

        var names = categories.Select(x => x.ToName()).ToArray();
        var descriptions = categoriesDesc.Select(x => x.ToDescription()).ToArray();
        Relations[] cats = relations.WithIndex().Select(x => new Relations(names[x.index], descriptions[x.index], x.item)).ToArray();
        var res = _classification.Reason(new(startPrompt.ToString(), queries, cats)).Result;

        Assert.True(!negatives.Any(x => res.Conclusion.Contains(x)));
        Assert.True(positives.All(x => res.Conclusion.Contains(x)));
    }


    [Theory]
    [InlineData("name", null)]
    [InlineData("name", "tag")]
    public void Should_convert_string_to_Name(string text, string? tag) 
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
    public void Should_convert_string_to_Description(string text, string? tag) 
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
    public void Should_convert_to_category_relation_prompt(string name, string nameTag, string description, string descTag, string relation, string prompt)
    {   

        Relations category = new(name.ToName(nameTag), description.ToDescription(descTag), relation);
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
    public void Should_extract_label(string text, string[] labels, string[] description, string[] expected)
    {
        var categories = new List<Relations>();
        foreach(var label in labels.WithIndex())
            categories.Add(new Relations(label.item.ToName(), description[label.index].ToDescription(), "sgesa" ));
        var actual = categories.Where(x => text.HasTag(x));
        Assert.Equal(expected.ToList(), actual.Select(x => x.Name.Text).ToList());
    }

}
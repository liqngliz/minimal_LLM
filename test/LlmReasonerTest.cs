using Context;
using IoC;
using Llm;
using Autofac;
using Reasoners;


namespace LlmReasonerTest;

[Collection("Sequential")]
public class LlmReasonerTest 
{
    readonly IReasoner<bool, Relevance> _relevanceReasoner;

    public LlmReasonerTest() 
    {
        var modules = new IoCModule("config.json");
        _relevanceReasoner = modules.Container().Resolve<IReasoner<bool, Relevance>>();
    }

    [Theory]
    [InlineData(
        "bkawfjdkjgbsdkhgjbskgbekjsbkejbsghjsegsbgebs ejskgbskgjbeskj", 
        "Fruits are the result of plant reproduction, they contain both nutrients and seeds that would allow a the plant's offspring to grow",
        "What is a fruit?",
        false
    )]

    [InlineData(
        "Follow your nose to delicious bursts of fruity flavor in Froot Loops sweetened multi-grain breakfast. Dig into vibrant, colorful crunchy O's made with tasty, natural fruit flavors and grains as the first ingredient. It's like a rainbow in every bowl. Fun to eat for adults and kids, this low-fat, its is a good source of 9 vitamins and minerals per serving. Not a Fruit but packs the same taste as fruits!", 
        "Fruits are the result of plant reproduction, they contain both nutrients and seeds that would allow a the plant's offspring to grow",
        "What is a fruit?",
        false
    )]

    [InlineData(
        "In Swiss law the general test of good faith is as such that a contract is an agreement between two or more parties to create one or more mutual obligations between them. To conclude a contract under Swiss Law, three conditions must be met: the parties to the contract must be capable of acting; the parties must have the intention of entering into a binding contract (the 'declaration of intent', i.e., offer and acceptance); the parties' declarations of intent must coincide (actually or normatively).", 
        "Fruits are the result of plant reproduction, they contain both nutrients and seeds that would allow a the plant's offspring to grow",
        "What is a good faith in contract law?",
        true
    )]

    [InlineData(
        "In Swiss law the general test of good faith is as such that a contract is an agreement between two or more parties to create one or more mutual obligations between them. To conclude a contract under Swiss Law, three conditions must be met: the parties to the contract must be capable of acting; the parties must have the intention of entering into a binding contract (the 'declaration of intent', i.e., offer and acceptance); the parties' declarations of intent must coincide (actually or normatively).", 
        "Even when in done in good faith, according to swiss law food importers must ensure by means of self-inspection that their goods comply with the legal requirements. The labels of foods intended for sale to consumers must contain all the necessary information.",
        "What is a good faith in contract law?",
        true
    )]

    public async void should_reason_relevance(string contentA, string contentB, string question, bool result)
    {
        var res = await _relevanceReasoner.Reason(new Relevance(contentA,contentB,question));
        Assert.Equal(result, res);
    }

}
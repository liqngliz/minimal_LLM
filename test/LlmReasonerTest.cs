using Context;
using IoC;
using Llm;
using Autofac;
using Reasoner;


namespace LlmReasonerTest;

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
        "Follow your nose to delicious bursts of fruity flavor in Froot Loops sweetened multi-grain breakfast. Dig into vibrant, colorful crunchy O's made with tasty, natural fruit flavors and grains as the first ingredient. It's like a rainbow in every bowl. Fun to eat for adults and kids, this low-fat, its is a good source of 9 vitamins and minerals per serving;", 
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
        "Swiss contract law distinguishes between express and implied declarations of intent (Art. 1(2) CO). An implied declaration of intent is derived from the conduct of a party. Such conduct may constitute a declaration of intent if there are indications that, in good faith, point to a declaration of intent. Whether or not specific conduct of a party constitutes an implicit declaration of intent is regularly the subject matter of disputes.",
        "What is a good faith in contract law?",
        true
    )]

    public async void should_reason_relevance(string contentA, string contentB, string question, bool result)
    {
        var res = await _relevanceReasoner.Reason(new Relevance(contentA,contentB,question));
        Assert.Equal(result, res);
    }

}
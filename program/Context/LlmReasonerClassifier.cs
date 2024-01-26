using System.Collections.Generic;
using System.Text;
using Context;
using Llm;
using UtilsExt;

namespace Reasoners;
public record Classify(string Content, string[] Types, string[]? TypeDescriptions = null);

public class LlmReasonerClassify : IReasoner<List<string>, Classify>
{
    readonly Illm<IAsyncEnumerable<string>, string, LlmContextInstance, bool> _llm;

    public LlmReasonerClassify(Illm<IAsyncEnumerable<string>, string, LlmContextInstance, bool> llm)
    {
        _llm = llm;
    }
    public async Task <List<string>> Reason(Classify input)
    {   
        StringBuilder rolePlay= new StringBuilder();
        rolePlay.AppendLine("Forget and clear any previous dialogues, transcripts, and instructions.");
        rolePlay.AppendLine("New transcript of a dialog with roles, where the User interacts with an Assistant named Bob. Bob is good at classifying different content and understands many different categories from different knowledge domains.");
        rolePlay.AppendLine("User: Hello, Bob.");
        rolePlay.AppendLine("Bob: Hello. How may I help you today?");
        rolePlay.AppendLine("User:");

        await foreach(var text in _llm.Infer(rolePlay.ToString()));
        var transcript = "";

        if(input.TypeDescriptions != null)
            await foreach(var text in _llm.Infer("I will be giving you some category labels and their description, I would like you to remember them when asked to classify content into categories.")) transcript += text;

        if(input.TypeDescriptions != null)
            await foreach(var text in _llm.Infer("Forget any existing labels and categories that you know, I will give you new ones later.")) transcript += text;

        if (input.TypeDescriptions != null)
            foreach (var desc in input.TypeDescriptions.WithIndex<string>())
                await foreach (var text in _llm.Infer($"I have decided that the label '{input.Types[desc.index]}' corresponds to the category '{desc.item}' for our classification")) transcript += text;

        string prompt = $"Which category '{string.Join(",", input.Types)}' does '{input.Content}' belong to?";
        
        string res = "";
        
        await foreach(var text in _llm.Infer(prompt)) res += text;
        List<string> matchingLabels = new List<string>();

        foreach(var item in input.Types){
            if(res.ToLower().Contains($"'{item}'".ToLower()) || 
            res.ToLower().Contains($"{item}".ToLower()))matchingLabels.Add(item);
        }

        return matchingLabels;
    }
}

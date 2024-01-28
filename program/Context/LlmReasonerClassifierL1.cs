using System.Collections.Generic;
using System.Text;
using Context;
using Llm;
using UtilsExt;

namespace Reasoners;

public record ClassifyL1(string Content, string[] Types);

public class LlmReasonerClassifyL1 : IReasoner<List<string>, ClassifyL1>
{
    readonly Illm<IAsyncEnumerable<string>, string, LlmContextInstance, bool> _llm;

    public LlmReasonerClassifyL1(Illm<IAsyncEnumerable<string>, string, LlmContextInstance, bool> llm)
    {
        _llm = llm;
    }
    public async Task <List<string>> Reason(ClassifyL1 input)
    {   
        StringBuilder rolePlay= new StringBuilder();
        rolePlay.AppendLine("Forget and clear any previous dialogues, transcripts, and instructions.");
        rolePlay.AppendLine("New transcript of a dialog with roles, where the User interacts with an Assistant named Bob. Bob is good at classifying different content and understands many different categories from different knowledge domains.");
        rolePlay.AppendLine("User: Hello, Bob.");
        rolePlay.AppendLine("Bob: Hello. How may I help you today?");
        rolePlay.AppendLine("User:");

        await foreach(var text in _llm.Infer(rolePlay.ToString()));
        var transcript = "";

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

using System.Collections.Generic;
using System.Text;
using Context;
using Llm;
using UtilsExt;

namespace Reasoners;

public record CategoryL2(string Name, string Description);
public record ClassifyL2(string Content,  CategoryL2[] Categories);

public class LlmReasonerClassifyL2 : IReasoner<List<string>, ClassifyL2>
{
    readonly Illm<IAsyncEnumerable<string>, string, LlmContextInstance, bool> _llm;

    public LlmReasonerClassifyL2(Illm<IAsyncEnumerable<string>, string, LlmContextInstance, bool> llm)
    {
        _llm = llm;
    }
    public async Task <List<string>> Reason(ClassifyL2 input)
    {   
        StringBuilder rolePlay= new StringBuilder();
        rolePlay.AppendLine("Forget and clear any previous dialogues, transcripts, and instructions.");
        rolePlay.AppendLine("New transcript of a dialog with roles, where the User interacts with an Assistant named Bob. Bob is good at classifying different content and understands many different categories from different knowledge domains.");
        rolePlay.AppendLine("User: Hello, Bob.");
        rolePlay.AppendLine("Bob: Hello. How may I help you today?");
        rolePlay.AppendLine("User:");

        await foreach(var text in _llm.Infer(rolePlay.ToString()));
        var transcript = "";

        string prompt = "I will be giving you some category labels and their description, I would like you to remember them when asked to classify content into categories.";
        transcript += prompt;
        await foreach(var text in _llm.Infer(prompt)) transcript += text;

        prompt = "Forget any existing labels and categories that you know, I will give you new ones later.";
        transcript += prompt;
        await foreach(var text in _llm.Infer(prompt)) transcript += text;


        foreach (var category in input.Categories)
        {   
            string chain = $"I have decided that the category label '{category.Name}' corresponds to the category '{category.Description}' for our classification";
            transcript += chain;
            await foreach (var text in _llm.Infer(chain)) transcript += text;
        }

        prompt = $"From the list '{string.Join(",", input.Categories.Select(x => x.Description))}', what are the possible categories for '{input.Content}'?";
  
        await foreach(var text in _llm.Infer(prompt)) transcript += text;
        
        string res = "";
        prompt = "Can you give me the corresponding labels?";

        await foreach(var text in _llm.Infer(prompt))res += text;
        transcript += res;
        
        List<string> matchingLabels = new List<string>();

        foreach(var item in input.Categories.Select(x => x.Name)){
            if(res.ToLower().Contains($"'{item}'".ToLower()) || 
            res.ToLower().Contains($"{item}".ToLower()))matchingLabels.Add(item);
        }

        return matchingLabels;
    }
}

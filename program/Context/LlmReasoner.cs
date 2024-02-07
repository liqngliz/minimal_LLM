using System.Collections.Generic;
using System.Text;
using Context;
using Llm;
using UtilsExt;

namespace Reasoners;

public record ReasonerTemplate(string Initial, string[] Queries, Relations[] Relations);
public record Name(string Text, string Tag);
public record Description(string Text, string Tag);
public record Relations(Name Name, Description Description, string Relation);
public record Reasoning(string Conclusion, List<string> Transcript, Relations[] Relations);
public class LlmReasoner : IReasoner<Reasoning, ReasonerTemplate>
{
    readonly Illm<IAsyncEnumerable<string>, string, LlmContextInstance, bool> _llm;

    public LlmReasoner(Illm<IAsyncEnumerable<string>, string, LlmContextInstance, bool> llm)
    {
        _llm = llm;
    }

    public async Task<Reasoning> Reason(ReasonerTemplate Input)
    {   
        var transcript = new List<string>();
        transcript.Add(Input.Initial);
        await foreach(var text in _llm.Infer(Input.Initial.ToString()));
        
        foreach(var category in Input.Relations)
        {
            string relation = category.ToRelationPrompt(); 
            transcript.Add(relation);
            var response = "";
            await foreach (var text in _llm.Infer(relation)) response += text;
            transcript.Add(response);
        }

        var results = new List<string>();

        foreach(var query in Input.Queries) {
            transcript.Add(query);
            var result = "";
            await foreach (var text in _llm.Infer(query)) result += text;
            transcript.Add(result);
            results.Add(result);
        }

        return new(results.Last(), transcript, Input.Relations);
    }
}

public static class ReasoningExtensions
{
    public static string ToRelationPrompt (this Relations category) =>  category.Relation.Replace(category.Name.Tag, category.Name.Text).Replace(category.Description.Tag, category.Description.Text);
    public static Name ToName(this string text, string tag = "{name}") => new Name(text, tag);
    public static Description ToDescription(this string text, string tag = "{description}") => new Description(text, tag);
    }

public static class ClassificationExtensions
{
    public static bool HasTag(this string content, Relations category) => content.ToLower().Contains(category.Name.Text.ToLower());
}
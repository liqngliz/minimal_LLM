using System.Collections.Generic;
using System.Text;
using Context;
using Llm;
using UtilsExt;

namespace Reasoners;

public record ClassificationTemplate(string Initial, string[] Queries, Category[] Categories, Func<string, string, bool> Func);

public record Category(Name Name, Description Description, string Relation);
public record Name(string Text, string Tag);
public record Description(string Text, string Tag);

public record Classification(List<Category> Categories, string Transcript);

public class LlmClassifier : IReasoner<Classification, ClassificationTemplate>
{
    readonly Illm<IAsyncEnumerable<string>, string, LlmContextInstance, bool> _llm;

    public LlmClassifier(Illm<IAsyncEnumerable<string>, string, LlmContextInstance, bool> llm)
    {
        _llm = llm;
    }

    public async Task<Classification> Reason(ClassificationTemplate Input)
    {   
        var transcript = "";
        transcript += Input.Initial;
        await foreach(var text in _llm.Infer(Input.Initial.ToString()));
        
        foreach(var category in Input.Categories)
        {
            string relation = category.ToRelationPrompt(); 
            transcript += relation;
            await foreach (var text in _llm.Infer(relation)) transcript += text;
        }

        var results = new List<string>();

        foreach(var query in Input.Queries) {
            transcript += query;
            var result = "";
            await foreach (var text in _llm.Infer(query)) result += text;
            transcript += result;
            results.Add(result);
        }

        var categories = Input.Categories.Where(x => Input.Func(x.Name.Text, results.Last()));

        return new(categories.ToList(), transcript);
    }
}

public static class ClassificationExtensions
{
    public static string ToRelationPrompt (this Category category) =>  category.Relation.Replace(category.Name.Tag, category.Name.Text).Replace(category.Description.Tag, category.Description.Text);
    public static Name ToName(this string text, string tag = "{name}") => new Name(text, tag);
    public static Description ToDescription(this string text, string tag = "{description}") => new Description(text, tag);

    public static bool HasTag(string tag, string content) => content.ToLower().Contains(tag.ToLower());
}
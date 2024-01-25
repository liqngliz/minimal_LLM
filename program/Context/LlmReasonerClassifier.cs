using System.Text;
using Context;
using Llm;
using UtilsExt;

namespace Reasoners;
public record Classify(string Content, string DefaultType, string[] Types, string[]? TypeDescriptions = null);

public class LlmReasonerClassify : IReasoner<string, Classify>
{
    readonly Illm<IAsyncEnumerable<string>, string, LlamaInstance, bool> _llm;

    public LlmReasonerClassify(Illm<IAsyncEnumerable<string>, string, LlamaInstance, bool> llm)
    {
        _llm = llm;
    }
    public async Task<string> Reason(Classify input)
    {
        StringBuilder rolePlay= new StringBuilder();
        rolePlay.AppendLine("Forget and clear any previous dialogues.");
        rolePlay.AppendLine("New transcript of a dialog with roles, where the User interacts with an Assistant named Bob. Bob is good at classifying different content and understands many different categories from different knowledge domains.");
        rolePlay.AppendLine("User: Hello, Bob.");
        rolePlay.AppendLine("Bob: Hello. How may I help you today?");
        rolePlay.AppendLine("User: I will be giving you some category labels and their description, I would like you to use them to classify some content?");
        rolePlay.AppendLine("Bob: Ok!");
        rolePlay.AppendLine("End of transcript start of new user input");
        rolePlay.AppendLine("User:");

        await foreach(var text in _llm.Infer(rolePlay.ToString()));

        if (input.TypeDescriptions != null)
            foreach (var desc in input.TypeDescriptions.WithIndex<string>())
                await foreach (var text in _llm.Infer($"'{input.Types[desc.index]}' is the category label for the '{desc.item}'")) ;

        string prompt = $"Which category '{string.Join(",", input.Types)}' does '{input.Content}' belong to. Limit answer to 5 words and use category labels.";
        
        string res = "";
        
        await foreach(var text in _llm.Infer(prompt)) res += text;

        _llm.Dispose();

        foreach(var item in input.Types)
            if(res.ToLower().Contains(item.ToLower())) return item;
        return input.DefaultType;
    }
}

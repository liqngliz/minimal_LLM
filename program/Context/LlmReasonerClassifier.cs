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
        rolePlay.AppendLine("Transcript of a dialog with roles, where the User interacts with an Assistant named Bob. Bob is helpful, kind, honest, and never fails to answer the User's requests immediately and with precision.");
        rolePlay.AppendLine("User: Hello, Bob.");
        rolePlay.AppendLine("Bob: Hello. How may I help you today?");
        rolePlay.AppendLine("User: Which category 'mechanical issue, user error, external factor, other' does 'I was driving my car and the all of a sudden the brakes stopped working! I had an orange indicator!' belong to.");
        rolePlay.AppendLine("Bob: mechanical issue");
        rolePlay.AppendLine("End of transcript start of new user input");
        rolePlay.AppendLine("User:");

        await foreach(var text in _llm.Infer(rolePlay.ToString()));

        if (input.TypeDescriptions != null)
            foreach (var desc in input.TypeDescriptions.WithIndex<string>())
                await foreach (var text in _llm.Infer($"Confirm that you understand {desc.item} is the definition of {input.Types[desc.index]}")) ;

        string prompt = $"Which category '{string.Join(",", input.Types)}' does '{input.Content}' belong to.";
        
        string res = "";
        
        await foreach(var text in _llm.Infer(prompt)) res += text;

        _llm.Dispose();

        foreach(var item in input.Types)
            if(res.Contains(item)) return item;
        return input.DefaultType;
    }
}

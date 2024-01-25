using System.Text;
using Context;
using Llm;

namespace Reasoners;
public record Relevance(string ContentA, string ContentB, string Question);
public class LlmReasonerRelevance : IReasoner<bool, Relevance>
{   
    readonly Illm<IAsyncEnumerable<string>, string, LlamaInstance, bool> _llm;

    public LlmReasonerRelevance(Illm<IAsyncEnumerable<string>, string, LlamaInstance, bool> llm)
    {
        _llm = llm;
    }

    public async Task<bool> Reason(Relevance Question)
    {   
        StringBuilder rolePlay= new StringBuilder();
        rolePlay.AppendLine("Forget and clear any previous dialogues.");
        rolePlay.AppendLine("New transcript of a dialog with roles, where the User interacts with an Assistant named Bob. Bob is helpful, kind, honest, good at writing, and never fails to answer the User's requests immediately and with precision.");
        rolePlay.AppendLine("User: Hello, Bob.");
        rolePlay.AppendLine("Bob: Hello. How may I help you today?");
        rolePlay.AppendLine("User: Answer with yes in single quotes only if the first text \"Swiss import laws on fruit state that there is an added tax on the value of fruit. Export fruits however do not have to pay any tax, rather they recieve a subsidy.\" more relevant than the second text \"Fruits are the result of plant reproduction, they contain both nutrients and seeds that would allow a the plant's offspring to grow\" for the question \"What is a fruit?\".");
        rolePlay.AppendLine("Bob: 'No'. The second text is more relevant as it directly explains that fruits are the result of plant reproduction, containing both nutrients and seeds for the plant's offspring. The first text is about a specific swiss law on fruits and doesn't address the general definition of fruits.");
        rolePlay.AppendLine("End of transcript start of new user input");
        rolePlay.AppendLine("User:");

        string prompt = $"Answer with yes in single quotes only if the first text \"{Question.ContentA}\" more pertinent content than the second text \"{Question.ContentB}\" for the question \"{Question.Question}\". Bob limit your answer to 150 characters.";
        
        string res = "";
        await foreach(var text in _llm.Infer(rolePlay.ToString()));
        await foreach(var text in _llm.Infer(prompt)) res += text;

        _llm.Dispose();
        return res.ToLowerInvariant().Contains("'yes'");
    }

}
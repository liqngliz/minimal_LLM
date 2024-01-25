using System.Text;
using Context;
using Llm;

namespace Reasoners;
public record Summary(string Content, int CharacterLimit);

public class LlmReasonerSummary : IReasoner<string, Summary>
{
    readonly Illm<IAsyncEnumerable<string>, string, LlamaInstance, bool> _llm;

    public LlmReasonerSummary(Illm<IAsyncEnumerable<string>, string, LlamaInstance, bool> llm)
    {
        _llm = llm;
    }
    public async Task<string> Reason(Summary Input)
    {
        StringBuilder rolePlay= new StringBuilder();
        rolePlay.AppendLine("Transcript of a dialog with roles, where the User interacts with an Assistant named Bob. Bob is helpful, kind, honest, good at writing, and never fails to answer the User's requests immediately and with precision.");
        rolePlay.AppendLine("User: Hello, Bob.");
        rolePlay.AppendLine("Bob: Hello. How may I help you today?");
        rolePlay.AppendLine("User: Can you provide a 150 character summary of the following \'First documented in 1147, Moscow grew to become a prosperous and powerful city that served as the capital of the Grand Duchy of Moscow. When the Tsardom of Russia was proclaimed, Moscow remained the political and economic center for most of its history. Under the reign of Peter the Great, the Russian capital was moved to the newly founded city of Saint Petersburg in 1712, diminishing Moscow's influence.\'");
        rolePlay.AppendLine("Bob: Moscow, documented in 1147, thrived as the Grand Duchy's capital. Despite the Tsardom, it retained political prominence until Peter the Great's shift to Saint Petersburg in 1712 weakened its influence.");
        rolePlay.AppendLine("End of transcript start of new user input");
        rolePlay.AppendLine("User:");

        string prompt = $"Can you provide a {Input.CharacterLimit} character summary of the following: \'{Input.Content}\'";
        
        string res = "";
        await foreach(var text in _llm.Infer(rolePlay.ToString()));
        await foreach(var text in _llm.Infer(prompt)) res += text;

        _llm.Dispose();

        return res.Replace("Bob:", "").Replace("User:", "");
    }
}
